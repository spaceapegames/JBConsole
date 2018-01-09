#!/usr/bin/env python
from subprocess import call
import os
import sys

os.chdir(sys.path[0])


# because file needs to be packages.config, put it in own directory
packagesFile = os.path.join("unity-JBConsole","packages.config")
if os.path.isfile(packagesFile):
	call(["nuget", "install",packagesFile,"-outputdirectory","packages"])

packagesFile = os.path.join("unity-JBConsole-editor","packages.config")
if os.path.isfile(packagesFile):
	call(["nuget", "install",packagesFile,"-outputdirectory","packages"])