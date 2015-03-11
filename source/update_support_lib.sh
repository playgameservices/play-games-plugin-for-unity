#!/bin/bash

pushd .

cd SupportLib

gradle clean
gradle build

popd

jar xvf SupportLib/PlayGamesPluginSupport/build/outputs/aar/PlayGamesPluginSupport-debug.aar classes.jar
cp -vf classes.jar PluginDev/Assets/Plugins/Android/MainLibProj/libs/play-games-plugin-support.jar
rm classes.jar

