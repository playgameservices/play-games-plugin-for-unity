### Directory for building new PGS Unity plugin as a UPM tarball

The new version of the PGS Unity plugin will be available to the public at the
same time as the legacy version. In order to maintain two independent versions
of the plugin, with the legacy plugin's codebase externally visible on GitHub,
and the new plugin requirement to build as a UPM-compatible tar.gz, the new
plugin is being maintained separately here.

This codebase does not sync with Git-on-Borg or GitHub and has its own gradle
build script.

During a gradle build, Assets files will be copied into the build with path
starting 'com.google.play.games/'.  Assets and SupportFiles directories have
been set up with Public/ and Protected/ areas so that a main build and an
'experimental' build can be maintained at the same time.

To build the plugin:

<section class="tabs">

#### Build plugin and sample apps {.new-tab}

```
cd third_party/unity/play_games_services/project/UPM
./gradlew
```

#### Build plugin only {.new-tab}

```
cd third_party/unity/play_games_services/project/UPM
./gradlew -PsampleBuild=off
```

#### Build experimental plugin {.new-tab}

This will pull in files from Assets/Protected/ and SupportFiles/Protected/
directories.

```
cd third_party/unity/play_games_services/project/UPM
./gradlew -PbuildMode=eap
```

</section>

Plugin unitypackage and tar.gz packages will be built in the build/ (or
build-eap/) directory that is added during the gradle build.

#### Testing

Unit tests found in project/UPM/Assets/Private/Tests are run as presubmits.

Changes to files within UPM/ will trigger Kokoro presubmits which take the Piper
code with changes and run gradle to build both .unitypackage and .tgz packages
for the plugin.  Then Kokoro builds the SmokeTest sample app APK and drops the
plugin and APK artifacts to x20.

TODO(cya): Add UI testing

TODO(cya): Add .csproj, .sln
