#!/bin/bash

for i in PluginDev ../samples/QuizRacer ../samples/TicTacToe; do
    if [ -d $i/Assets/Plugins/Android ]; then
        cp -vf SupportLib/BaseGameUtils/build/bundles/debug/classes.jar $i/Assets/Plugins/Android/BaseGameUtils/libs/base-game-utils.jar

        cp -vf SupportLib/PlayGamesPluginSupport/build/bundles/debug/classes.jar $i/Assets/Plugins/Android/MainLibProj/libs/play-games-plugin-support.jar
    else
        "*** Skipping $i because $i/Assets/Plugins/Android does not exist."
    fi
done


