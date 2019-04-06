#!/usr/bin/python
import copy
import os
import subprocess
import sys
import traceback


class Builder(object):
    """Class that represents the actions for a builder"""
    def __init__(self, repeat_no_clean):
        self.repeat_no_clean = repeat_no_clean

    def init(self, options):
        pass

    def pre_action(self):
        pass

    def post_action(self, package, options, flavour, output_messages, error_messages):
        return 0


# the version of MSBuild.exe to use, depends on which version of VisualStudio
# was used to build the solution and projects
# by default, VS2013 is assumed
defaultVCVersion = "12.0"
msBuildVersionToNetMapping = {
    "9.0": "v3.5",
    "10.0": "v4.0.30319",
    "11.0": "v4.0.30319",
    "12.0": "v4.0.30319",
    "14.0": "14.0",
    "15.0": "15.0",
    "16"  : "Current"
}
visualStudioVersionMapping = {
    "15.0": "2017",
    "16"  : "2019"
}


class NativeBuilder(Builder):
    def __init__(self):
        super(NativeBuilder, self).__init__(True)


class VSSolutionBuilder(Builder):
    def __init__(self):
        super(VSSolutionBuilder, self).__init__(False)
        self._ms_build_path = None

    def _get_visualc_version(self, options):
        try:
            for f in options.Flavours:
                if f.startswith("--VisualC.version"):
                    visualc_version = f.split("=")[1]
                    break
        except TypeError:  # Flavours can be None
            pass
        try:
            visualc_version_split = visualc_version.split('.')
        except UnboundLocalError:
            visualc_version = defaultVCVersion
            visualc_version_split = visualc_version.split('.')
        return visualc_version, visualc_version_split

    def _get_visualc_ispreview(self, options):
        try:
            for f in options.Flavours:
                if f.startswith("--VisualC.discoverprereleases"):
                    return True
        except:
            return False

    def init(self, options):
        visualc_version, visualc_version_split = self._get_visualc_version(options)
        visualc_major_version = int(visualc_version_split[0])
        # location of MSBuild changed in VS2013, and VS2017
        if visualc_major_version >= 15:
            visualStudioVersion = visualStudioVersionMapping[visualc_version]
            msbuild_version = msBuildVersionToNetMapping[visualc_version]
            edition = "Preview" if self._get_visualc_ispreview(options) else "Community"
            if os.environ.has_key("ProgramFiles(x86)"):
                self._ms_build_path = r"C:\Program Files (x86)\Microsoft Visual Studio\%s\%s\MSBuild\%s\Bin\MSBuild.exe" % (visualStudioVersion, edition, msbuild_version)
            else:
                self._ms_build_path = r"C:\Program Files (x86)\Microsoft Visual Studio\%s\%s\MSBuild\%s\Bin\amd64\MSBuild.exe" % (visualStudioVersion, edition, msbuild_version)
        elif visualc_major_version >= 12:
            # VS2013 onwards path for MSBuild
            if os.environ.has_key("ProgramFiles(x86)"):
                self._ms_build_path = r"C:\Program Files (x86)\MSBuild\%s\bin\MSBuild.exe" % visualc_version
            else:
                self._ms_build_path = r"C:\Program Files\MSBuild\%s\bin\MSBuild.exe" % visualc_version
        else:
            self._ms_build_path = r"C:\Windows\Microsoft.NET\Framework\%s\MSBuild.exe" % msBuildVersionToNetMapping[visualc_version]

    def post_action(self, package, options, flavour, output_messages, error_messages):
        exit_code = 0
        build_root = os.path.join(package.get_path(), options.buildRoot)
        solution_path = os.path.join(build_root, package.get_id() + ".sln")
        if not os.path.exists(solution_path):
            # TODO: really need something different here - an invalid test result, rather than a failure
            output_messages.write("VisualStudio solution expected at %s did not exist" % solution_path)
            return 0
        try:
            for config in options.configurations:
                arg_list = [
                    self._ms_build_path,
                    "/verbosity:normal",
                    solution_path
                ]
                # capitalize the first letter of the configuration
                config = config[0].upper() + config[1:]
                arg_list.append("/p:Configuration=%s" % config)
                for platform in flavour.platforms():
                    this_arg_list = copy.deepcopy(arg_list)
                    this_arg_list.append("/p:Platform=%s" % platform)
                    output_messages.write("Running '%s'\n" % ' '.join(this_arg_list))
                    try:
                        p = subprocess.Popen(this_arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
                        (output_stream, error_stream) = p.communicate()  # this should WAIT
                        exit_code |= p.returncode
                        if output_stream:
                            output_messages.write(output_stream)
                        if error_stream:
                            error_messages.write(error_stream)
                    except WindowsError:
                        error_messages.write("Failed to run '%s'" % ' '.join(this_arg_list))
                        raise
        except Exception, e:
            import traceback
            error_messages.write(str(e) + '\n' + traceback.format_exc())
            return -1
        return exit_code


class MakeFileBuilder(Builder):
    def __init__(self):
        super(MakeFileBuilder, self).__init__(False)
        self._make_executable = 'make'
        self._make_args = []

    def init(self, options):
        if sys.platform.startswith("win"):
            arg_list = [
                'where',
                '/R',
                os.path.expandvars('%ProgramFiles(x86)%'),
                'nmake.exe'
            ]
            p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            (output_stream, error_stream) = p.communicate()  # this should WAIT
            if output_stream:
                split = output_stream.splitlines()
                self._make_executable = split[0].strip()
                self._make_args.append('-NOLOGO')

    def post_action(self, package, options, flavour, output_messages, error_messages):
        exit_code = 0
        makefile_dir = os.path.join(package.get_path(), options.buildRoot)
        if not os.path.exists(makefile_dir):
            # TODO: really need something different here - an invalid test result, rather than a failure
            output_messages.write("Expected folder containing MakeFile %s did not exist" % makefile_dir)
            return 0
        try:
            # currently do not support building configurations separately
            arg_list = [
                self._make_executable
            ]
            arg_list.extend(self._make_args)
            print "Running '%s' in %s\n" % (' '.join(arg_list), makefile_dir)
            p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=makefile_dir)
            (output_stream, error_stream) = p.communicate()  # this should WAIT
            exit_code |= p.returncode
            if output_stream:
                output_messages.write(output_stream)
            if error_stream:
                error_messages.write(error_stream)
        except Exception, e:
            error_messages.write(str(e))
            return -1
        return exit_code


class XcodeBuilder(Builder):
    def __init__(self):
        super(XcodeBuilder, self).__init__(False)

    def post_action(self, package, options, flavour, output_messages, error_messages):
        exit_code = 0
        build_root = os.path.join(package.get_path(), options.buildRoot)
        xcode_workspace_path = os.path.join(build_root, "*.xcworkspace")
        import glob
        workspaces = glob.glob(xcode_workspace_path)
        if not workspaces:
            # TODO: really need something different here - an invalid test result, rather than a failure
            output_messages.write("Xcode workspace expected at %s did not exist" % xcode_workspace_path)
            return 0
        if len(workspaces) > 1:
            output_messages.write("More than one Xcode workspace was found")
            return -1
        try:
            # first, list all the schemes available
            arg_list = [
                "xcodebuild",
                "-workspace",
                workspaces[0],
                "-list"
            ]
            print "Running '%s'\n" % ' '.join(arg_list)
            p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
            (output_stream, error_stream) = p.communicate()  # this should WAIT
            output_messages.write(output_stream)
            error_messages.write(error_stream)
            # parse the output to get the schemes
            lines = output_stream.split('\n')
            if len(lines) < 3:
                raise RuntimeError("Unable to parse workspace for schemes. \
                                   Was --Xcode.generateSchemes passed to the Bam build?")
            schemes = []
            has_schemes = False
            for line in lines:
                trimmed = line.strip()
                if has_schemes:
                    if trimmed:
                        schemes.append(trimmed)
                elif trimmed.startswith('Schemes:'):
                    has_schemes = True
            if not has_schemes or len(schemes) == 0:
                raise RuntimeError("No schemes were extracted from the workspace. \
                                Has the project scheme cache been warmed?")
            # iterate over all the schemes and configurations
            for scheme in schemes:
                for config in options.configurations:
                    arg_list = [
                        "xcodebuild",
                        "-workspace",
                        workspaces[0],
                        "-scheme",
                        scheme,
                        "-configuration"
                    ]
                    # capitalize the first letter of the configuration
                    config = config[0].upper() + config[1:]
                    arg_list.append(config)
                    print "Running '%s' in %s" % (" ".join(arg_list), build_root)
                    output_messages.write("Running '%s' in %s" % (" ".join(arg_list), build_root))
                    p = subprocess.Popen(arg_list, stdout=subprocess.PIPE, stderr=subprocess.PIPE, cwd=build_root)
                    (output_stream, error_stream) = p.communicate()  # this should WAIT
                    exit_code |= p.returncode
                    if output_stream:
                        output_messages.write(output_stream)
                    if error_stream:
                        error_messages.write(error_stream)
        except Exception, e:
            error_messages.write("%s\n" % str(e))
            error_messages.write(traceback.format_exc())
            return -1
        return exit_code


builder = {
    "Native": NativeBuilder(),
    "VSSolution": VSSolutionBuilder(),
    "MakeFile": MakeFileBuilder(),
    "Xcode": XcodeBuilder()
}


def get_builder_details(builder_name):
    """Return the Builder associated with the name passed in
    Args:
        builder_name:
    """
    return builder[builder_name]
