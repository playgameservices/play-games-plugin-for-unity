#!/usr/bin/python

from mod_pbxproj import *
from os import path, listdir
from shutil import copytree
import sys
import xml.etree.ElementTree as ET
import plistlib

script_dir = os.path.dirname(sys.argv[0])
build_path = sys.argv[1]
meta_data = sys.argv[2]

print ("script_dir:{0}".format(script_dir))
print ("build_path:{0}".format(build_path))
print ("metadata:{0}".format(meta_data))

pbx_file_path = build_path + '/Unity-iPhone.xcodeproj/project.pbxproj'
pbx_object = XcodeProject.Load(pbx_file_path)

google_framework_dir = path.join(script_dir,'..','..','XCodeFiles', 'ios-google-play-games')
target_google_framework_dir = path.join(build_path, 'Libraries', 'ios-google-play-games')
copytree(google_framework_dir, target_google_framework_dir)
pbx_object.add_framework_search_paths([path.abspath(target_google_framework_dir)])
pbx_object.add_header_search_paths([path.abspath(target_google_framework_dir)])
google_framework = path.join(target_google_framework_dir, 'GooglePlayGames.framework')
pbx_object.add_file_if_doesnt_exist(path.abspath(google_framework), tree='SDKROOT')
gpg_framework = path.join(target_google_framework_dir, 'gpg.framework')
pbx_object.add_file_if_doesnt_exist(path.abspath(gpg_framework), tree='SDKROOT')
google_resource_bundle = path.join(target_google_framework_dir, 'GooglePlayGames.bundle')
pbx_object.add_file_if_doesnt_exist(path.abspath(google_resource_bundle), tree='SDKROOT')

pbx_object.add_other_ldflags('-ObjC')

pbx_object.save()

plist_data = plistlib.readPlist(os.path.join(build_path, 'Info.plist'))
plist_types_arr = plist_data.get("CFBundleURLTypes")

if plist_types_arr == None:
    plist_types_arr = []
    plist_data["CFBundleURLTypes"] = plist_types_arr

plistlib.writePlist(plist_data, os.path.join(build_path, 'Info.plist'))
