#!/usr/bin/python

from optparse import OptionParser
import os
import platform
import subprocess
import sys

system = platform.system()
if system == 'Windows':
    bam_shell = 'bam.bat'
else:
    bam_shell = 'bam'


def _log(message):
    print >>sys.stdout, message
    sys.stdout.flush()


def get_bam_installdir():
    try:
        return subprocess.check_output([bam_shell, '--installdir']).rstrip()
    except WindowsError:
        raise RuntimeError('Unable to locate BAM on the PATH')


def get_bam_rootdir():
    install_dir = get_bam_installdir()
    root_dir = os.path.realpath(os.path.join(install_dir, os.pardir, os.pardir, os.pardir))
    return root_dir


def get_bam_testsdir():
    root_dir = get_bam_rootdir()
    tests_dir = os.path.join(root_dir, 'tests')
    return tests_dir


def add_dependent(package_dir, name, version):
    current_dir = os.getcwd()
    try:
        _log("Updating '%s'" % package_dir)
        os.chdir(package_dir)
        args = [bam_shell, '--adddependent', '--pkgname=%s' % name]
        if version:
            args.append('--pkgversion=%s' % version)
        process = subprocess.Popen(args)
        process.wait()
    finally:
        os.chdir(current_dir)


def process_path(tests_dir, name, version):
    for test_name in os.walk(tests_dir).next()[1]:
        test_package = os.path.join(tests_dir, test_name)
        add_dependent(test_package, name, version)


if __name__ == "__main__":
    parser = OptionParser()
    parser.add_option('--name')
    parser.add_option('--version', default=None)
    (options, args) = parser.parse_args()
    if not options.name:
        parser.error('Package name is required')
    tests_dir = get_bam_testsdir()
    process_path(tests_dir, options.name, options.version)
