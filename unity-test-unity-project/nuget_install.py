import os
from xml.etree import ElementTree as ET
import shutil

nugetdir = 'Assets/Plugins/nuget'
packagesconfig = 'packages.config'
nugetexecutable = 'NuGet.exe'
debug_logging = False
errors = []

def debug_print(message):
    """ Conditionally prints out messages based on debug_logging value."""
    if debug_logging:
        print(message)


def error(message, print_now=False):
    """ Prints out an error, and appends to the global errors variable."""
    errors.append(message)
    if print_now:
        print(message)


def delete_all_non_meta_files_in_folder(folder):
    """ Recursively deletes all files which are not meta files in the target folder."""
    for dirname, dirnames, filenames in os.walk(folder):
        debug_print('Deleting files in ' + folder)

        # delete all non meta files
        for file in filenames:
            if not file.endswith('.meta') and file != packagesconfig and not file.endswith(nugetexecutable):
                if debug_logging:
                    print(file)
                os.remove(os.path.join(folder, file))

        # now go through all the sub folders
        for subdirname in dirnames:
            subdir = os.path.join(dirname, subdirname)
            delete_all_non_meta_files_in_folder(subdir)


def nuget_install():
    """ Installs the nuget dependencies from your packages.config file excluding their versions."""
    command = 'nuget install ' + nugetdir + '/' + packagesconfig + ' -excludeversion -outputdirectory ' + nugetdir
    os.system(command)


def remove_unused_nuget_targets():
    """ Checks your packages.config, and removes all but the used target version of each dependency.
        If your package does not list a target then it will skip it.
        Packages which do not exist will be listed at the end of the process.
    """
    xml = ET.parse(nugetdir + '/' + packagesconfig)
    packages_element = xml.getroot()
    for package_element in packages_element:
        id = package_element.attrib['id']
        version = package_element.attrib['version']

        target = ''
        if 'target' in package_element.attrib:
            target = package_element.attrib['target']
        elif 'targetFramework' in package_element.attrib:
            target = package_element.attrib['targetFramework']

        if target != '':
            path_to_frameworks = nugetdir + '/' + id + '/' + 'lib'
            found_target = False
            directories_to_remove = []
            # Look for the target we want.
            for subdir, directories, files in os.walk(path_to_frameworks):
                for directory in directories:
                    if directory == target:
                        found_target = True
                    else:
                        directories_to_remove.append(os.path.join(subdir, directory))
                break  # Important as it stops the depth at 1

            if not found_target:
                error('ERROR: Failed to find target ' + target + ' for ' + id);
            else:
                # If we found the target we want, we'll remove all the others.
                for directory_to_remove in directories_to_remove:
                    debug_print('Removing unneeded target ' + directory_to_remove)
                    shutil.rmtree(directory_to_remove)
        else:
            error('WARN: Dependency \'' + id + ':' + version + '\' had not target.', print_now=True)


if __name__ == '__main__':
    print('Deleting all non meta files...')

    delete_all_non_meta_files_in_folder(nugetdir)

    nuget_install()

    remove_unused_nuget_targets()

    print

    if len(errors) > 0:
        print('Summary of errors:')
        for error in errors:
            print('\t' + error)
    else:
        print('No errors')