#!/bin/sh

if ! [ -d '.git' ]; then
    if [ -d '../.git' ]; then
        cd ..
    else
        echo "*** Run this from the GIT root directory."
        exit 1
    fi
fi


ln -sf ../../git-hooks/pre-commit .git/hooks/pre-commit
chmod +x git-hooks/*
