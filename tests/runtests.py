#!/usr/bin/python

from builderactions import get_builder_details
import copy
import glob
import imp
from optparse import OptionParser
import os
import StringIO
import subprocess
import sys
from testconfigurations import test_option_setup
from testinstance import TestInstance
import time
import xml.etree.ElementTree as ET

# ----------

if sys.platform.startswith("win"):
    bam_shell = 'bam.bat'
else:
    bam_shell = 'bam'


def print_message(message):
    print >>sys.stdout, message
    sys.stdout.flush()


class Package:
    def __init__(self, root, name, version):
        self.root = root
        self.name = name
        self.version = version
        self.repo = None
        self.package_dir = None

    @classmethod
    def from_xml(cls, xml_filename):
        document = ET.parse(xml_filename)
        root = document.getroot()
        instance = cls(None, root.attrib["name"], root.attrib.get("version", None))
        instance.package_dir = os.path.normpath(os.path.join(xml_filename, os.pardir, os.pardir))
        instance.repo = os.path.normpath(os.path.join(instance.package_dir, os.pardir))
        return instance

    def get_description(self):
        if self.version:
            return "%s-%s in %s" % (self.name, self.version, self.repo)
        else:
            return "%s in %s" % (self.name, self.repo)

    def get_path(self):
        return self.package_dir

    def get_id(self):
        if self.version:
            return "-".join([self.name, self.version])
        else:
            return self.name

    def get_name(self):
        return self.name

# ----------


def find_all_packages_to_test(root, options):
    """Locate packages that can be tested
    Args:
        root:
        options:
    """
    if options.verbose:
        print_message("Locating packages under '%s'" % root)
    tests = []
    dirs = os.listdir(root)
    dirs.sort()
    for packageName in dirs:
        if packageName.startswith("."):
            continue
        package_dir = os.path.join(root, packageName)
        if not os.path.isdir(package_dir):
            continue
        bam_dir = os.path.join(package_dir, 'bam')
        if not os.path.isdir(bam_dir):
            continue
        xml_files = glob.glob(os.path.join(bam_dir, "*.xml"))
        if len(xml_files) == 0:
            continue
        if len(xml_files) > 1:
            raise RuntimeError("Too many XML files found in %s to identify a package definition file" % bam_dir)
        package = Package.from_xml(xml_files[0])
        if options.verbose:
            print_message("\t%s" % package.get_id())
        tests.append(package)
    return tests


def _init_builder(builder, options):
    builder.init(options)


def _pre_execute(builder):
    builder.pre_action()


def _run_buildamation(options, instance, extra_args, output_messages, error_messages):
    arg_list = [
        bam_shell,
        "-o=%s" % options.buildRoot,
        "-b=%s" % options.buildmode
    ]
    for config in options.configurations:
        arg_list.append("--config=%s" % config)
    arg_list.append("-j=" + str(options.numJobs))
    if options.debugSymbols:
        arg_list.append("-d")
    if options.verbose:
        arg_list.append("-v=2")
    else:
        arg_list.append("-v=0")
    if options.forceDefinitionUpdate:
        arg_list.append("--forceupdates")
    if not options.noInitialClean:
        arg_list.append("--clean")
    if extra_args:
        arg_list.extend(extra_args)
    if options.injected:
        for inject in options.injected:
            arg_list.append("--injectdefaultpackage=%s" % inject)
    print_message(" ".join(arg_list))
    p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=instance.package_path())
    (output_stream, error_stream) = p.communicate()  # this should WAIT
    if output_stream:
        output_messages.write(output_stream)
    if error_stream:
        error_messages.write(error_stream)
    return p.returncode, arg_list


def _post_execute(builder, options, flavour, instance, output_messages, error_messages):
    if options.dumpprojects:
        builder.dump_generated_files(instance, options)
    exit_code = builder.post_action(instance, options, flavour, output_messages, error_messages)
    return exit_code


class Stats(object):
    def __init__(self):
        self._total = 0
        self._success = 0
        self._fail = []
        self._ignore = []


def execute_test_instance(instance, options, output_buffer, stats, the_builder):
    print_message(128 * '=')
    if instance.runnable():
        print_message("* Running  %s\t%s\t%s" % (instance.package_name(), instance.flavour(), ' '.join(instance.variation_arguments())))
        if options.excludedVariations:
            print_message(" (excluding %s)" % options.excludedVariations)
    else:
        print_message("* Ignoring %s\t%s" % (instance.package_name(), instance.flavour()))
        output_buffer.write("IGNORED: Test instance '%s' is not runnable in this configuration\n" % str(instance))
        stats._total += 1
        stats._ignore.append(str(instance))
        return 0

    if options.verbose:
        print_message("\tPackage Description : %s" % instance.package_description())

    non_kwargs = []
    exit_code = 0
    stats._total += 1
    iterations = 1

    for it in range(0, iterations):
        extra_args = non_kwargs[:]
        if options.Flavours:
            extra_args.extend(options.Flavours)
        extra_args.extend(instance.variation_arguments())
        try:
            output_messages = StringIO.StringIO()
            error_messages = StringIO.StringIO()
            start_time = os.times()
            _pre_execute(the_builder)
            returncode, arg_list = _run_buildamation(options, instance, extra_args, output_messages, error_messages)
            if returncode == 0:
                if the_builder.repeat_no_clean:
                    no_clean_options = copy.deepcopy(options)
                    no_clean_options.noInitialClean = True
                    returncode, _ = _run_buildamation(no_clean_options, instance, extra_args, output_messages, error_messages)
                if returncode == 0:
                    returncode = _post_execute(the_builder, options, variation, instance, output_messages, error_messages)
            end_time = os.times()
        except Exception, e:
            print_message("Popen exception: '%s'" % str(e))
            raise
        finally:
            message = "Test instance '%s'" % str(instance)
            if extra_args:
                message += " with extra arguments '%s'" % " ".join(extra_args)
            message += " executed in (%f,%f,%f,%f,%f) seconds" %\
                (
                    end_time[0] - start_time[0],
                    end_time[1] - start_time[1],
                    end_time[2] - start_time[2],
                    end_time[3] - start_time[3],
                    end_time[4] - start_time[4]
                )
            try:
                if returncode == 0:
                    stats._success += 1
                    output_buffer.write("SUCCESS: %s\n" % message)
                    if options.verbose:
                        if len(output_messages.getvalue()) > 0:
                            output_buffer.write("Messages:\n")
                            output_buffer.write(output_messages.getvalue())
                        if len(error_messages.getvalue()) > 0:
                            output_buffer.write("Errors:\n")
                            output_buffer.write(error_messages.getvalue())
                else:
                    stats._fail.append(str(instance))
                    output_buffer.write("* FAILURE *: %s\n" % message)
                    output_buffer.write("Command was: %s\n" % " ".join(arg_list))
                    output_buffer.write("Executed in: %s\n" % instance.package_path())
                    if len(output_messages.getvalue()) > 0:
                        output_buffer.write("Messages:\n")
                        output_buffer.write(output_messages.getvalue())
                    if len(error_messages.getvalue()) > 0:
                        output_buffer.write("Errors:\n")
                        output_buffer.write(error_messages.getvalue())
                    output_buffer.write("\n")
                    exit_code -= 1
            except UnboundLocalError:  # for returncode
                message += "... did not complete due to earlier errors"
    return exit_code


def clean_up(options):
    arg_list = []
    cur_dir = os.path.dirname(os.path.realpath(__file__))
    if sys.platform.startswith("win"):
        arg_list.append(os.path.join(cur_dir, "removedebugprojects.bat"))
        arg_list.append("-nopause")
    else:
        arg_list.append(os.path.join(cur_dir, "removedebugprojects.sh"))
    if options.verbose:
        print_message("Executing: %s" % arg_list)
    p = subprocess.Popen(arg_list)
    p.wait()


def find_bam_default_repository():
    bam_install_dir = subprocess.check_output([bam_shell, '--installdir']).rstrip()
    repo_dir = os.path.realpath(os.path.join(bam_install_dir, os.pardir, os.pardir, os.pardir))
    return repo_dir

# ----------

if __name__ == "__main__":
    bam_dir = find_bam_default_repository()

    optParser = OptionParser(description="BuildAMation unittests")
    # optParser.add_option("--platform", "-p", dest="platforms", action="append", default=None, help="Platforms to test")
    optParser.add_option("--configuration", "-c", dest="configurations", type="choice", choices=["debug","profile","optimized"], action="append", default=None, help="Configurations to test")
    optParser.add_option("--test", "-t", dest="tests", action="append", default=None, help="Tests to run")
    optParser.add_option("--excludetest", "-T", dest="xtests", action="append", default=None, help="Tests to not run")
    optParser.add_option("--buildroot", "-o", dest="buildRoot", action="store", default="build", help="BuildAMation build root")
    optParser.add_option("--buildmode", "-b", dest="buildmode", type="choice", choices=["Native", "VSSolution", "MakeFile", "Xcode"], action="store", default="Native", help="BuildAMation build mode to test")
    optParser.add_option("--keepfiles", "-k", dest="keepFiles", action="store_true", default=False, help="Keep the BuildAMation build files around")
    optParser.add_option("--jobs", "-j", dest="numJobs", action="store", type="int", default=1, help="Number of jobs to use with BuildAMation builds")
    optParser.add_option("--verbose", "-v", dest="verbose", action="store_true", default=False, help="Verbose output")
    optParser.add_option("--debug", "-d", dest="debugSymbols", action="store_true", default=False, help="Build BuildAMation packages with debug information")
    optParser.add_option("--noinitialclean", "-i", dest="noInitialClean", action="store_true", default=False, help="Disable cleaning packages before running tests")
    optParser.add_option("--forcedefinitionupdate", "-f", dest="forceDefinitionUpdate", action="store_true", default=False, help="Force definition file updates")
    optParser.add_option("--excludevariation", "-x", dest="excludedVariations", action="append", default=None, help="Exclude a variation from the test configurations")
    optParser.add_option("--C.bitdepth", dest="bitDepth", type="choice", choices=["*", "32", "64"], action="store", default="*", help="Build bit depth to test")
    optParser.add_option("--repo", "-r", dest="repos", action="append", default=[bam_dir], help="Add a package repository to test")
    optParser.add_option("--nodefaultrepo", dest="nodefaultrepo", action="store_true", default=False, help="Do not test the default repository")
    optParser.add_option("--injectdefaultpackage", dest="injected", action="append", default=None, help="Inject default packages, specify packagename or packagename-packageversion")
    optParser.add_option("--dumpprojects", dest="dumpprojects", action="store_true", default=False, help="Dump generated project files to stdout")
    test_option_setup(optParser)
    (options, args) = optParser.parse_args()

    if options.nodefaultrepo:
        options.repos.remove(bam_dir)
        if not options.repos:
            raise RuntimeError("No package repositories to test")

    if options.verbose:
        print_message("Options are %s" % options)
        print_message("Args    are %s" % args)

    # if not options.platforms:
    #    raise RuntimeError("No platforms were specified")

    if not options.configurations:
        raise RuntimeError("No configurations were specified")

    # if not options.noInitialClean:
    #    clean_up(options)

    exit_code = 0
    for repo in options.repos:
        if not os.path.isabs(repo):
            repo = os.path.join(os.getcwd(), repo)
        repoTestDir = os.path.join(repo, "tests")
        bamTestsConfigPathname = os.path.join(repoTestDir, 'bamtests.py')
        if not os.path.isfile(bamTestsConfigPathname):
            print_message("Package repository %s has no bamtests.py file" % repo)
            continue
        bamtests = imp.load_source('bamtests', bamTestsConfigPathname)
        testConfigs = bamtests.configure_repository()
        tests = find_all_packages_to_test(repoTestDir, options)
        if options.tests:
            if options.verbose:
                print_message("Tests to run are: %s" % options.tests)
            filteredTests = []
            for test in options.tests:
                found = False
                for package in tests:
                    if package.get_id() == test:
                        filteredTests.append(package)
                        found = True
                        break
                if not found:
                    raise RuntimeError("Unrecognized package '%s'" % test)
            tests = filteredTests
        if options.xtests:
            if options.verbose:
                print_message("Tests to not run are: %s" % options.xtests)
            tests = [t for t in tests if not t.get_id() in options.xtests]

        test_instances = set()
        stats = Stats()
        for test in tests:
            try:
                configs = testConfigs[test.get_name()]
            except KeyError, e:
                if options.verbose:
                    print_message("No configuration for package %s: %s" % (test.get_name(), str(e)))
                continue
            try:
                variations = configs.get_variations(options.buildmode, options.excludedVariations, options.bitDepth)
            except KeyError:
                variations = [None]
            for variation in variations:
                test_instances.add(TestInstance(test, '+'.join(set(options.configurations)), variation))
        if options.verbose:
            print "Test instances are:"
            for instance in sorted(test_instances, key=lambda instance: str(instance)):
                print "\t%s" % str(instance)

        # builder gets constructed once, for all packages
        the_builder = get_builder_details(options.buildmode)
        _init_builder(the_builder, options)

        output_buffer = StringIO.StringIO()
        for test in sorted(test_instances, key=lambda instance: str(instance)):
            exit_code += execute_test_instance(test, options, output_buffer, stats, the_builder)

        if not options.keepFiles:
            # TODO: consider keeping track of all directories created instead
            clean_up(options)

        print_message("--------------------")
        print_message("| Results summary  |")
        print_message("--------------------")
        print_message(output_buffer.getvalue())
        print_message("Success %s/%s" % (stats._success, stats._total))
        print_message("Failure %s/%s" % (len(stats._fail), stats._total))
        if stats._fail:
            for failed in stats._fail:
                print_message("\t%s" % failed)
        print_message("Ignored %s/%s" % (len(stats._ignore), stats._total))
        if stats._ignore:
            for ignored in stats._ignore:
                print_message("\t%s" % ignored)

        logsDir = os.path.join(repoTestDir, "Logs")
        if not os.path.isdir(logsDir):
            os.makedirs(logsDir)
        logFileName = os.path.join(logsDir, "tests_" + time.strftime("%d-%m-%YT%H-%M-%S") + ".log")
        logFile = open(logFileName, "w")
        logFile.write(output_buffer.getvalue())
        logFile.close()
        output_buffer.close()

    sys.exit(exit_code)
