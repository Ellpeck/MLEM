#addin nuget:?package=Cake.DocFx&version=1.0.0
#tool dotnet:?package=docfx&version=2.70.3

// this is the upcoming version, for prereleases
var version = Argument("version", "6.3.0");
var target = Argument("target", "Default");
var branch = Argument("branch", "main");
var config = Argument("configuration", "Release");
var serve = HasArgument("serve");

Task("Prepare").Does(() => {
    DotNetWorkloadInstall("android");
    DotNetRestore("MLEM.sln");
    DotNetRestore("MLEM.FNA.sln");

    if (branch != "release") {
        var buildNum = EnvironmentVariable("CI_PIPELINE_NUMBER");
        if (!string.IsNullOrEmpty(buildNum))
            version += "-ci." + buildNum;
    }

    DeleteFiles("**/MLEM*.nupkg");
});

Task("Build").IsDependentOn("Prepare").Does(() =>{
    var settings = new DotNetBuildSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}"),
        // .net 8 has an issue that causes simultaneous tool restores during build to fail
        MSBuildSettings = new DotNetMSBuildSettings { MaxCpuCount = 1 }
    };
    DotNetBuild("MLEM.sln", settings);
    DotNetBuild("MLEM.FNA.sln", settings);
});

Task("Test").IsDependentOn("Build").Does(() => {
    var settings = new DotNetTestSettings {
        Configuration = config,
        Collectors = {"XPlat Code Coverage"},
        Loggers = {"console;verbosity=normal"}
    };
    DotNetTest("MLEM.sln", settings);
    DotNetTest("MLEM.FNA.sln", settings);
});

Task("Pack").IsDependentOn("Test").Does(() => {
    var settings = new DotNetPackSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    };
    DotNetPack("MLEM.sln", settings);
    DotNetPack("MLEM.FNA.sln", settings);
});

Task("Push")
    .WithCriteria(branch == "main" || branch == "release", "Not on main or release branch")
    .WithCriteria(string.IsNullOrEmpty(EnvironmentVariable("CI_COMMIT_PULL_REQUEST")), "On pull request")
    .IsDependentOn("Pack").Does(() => {
        DotNetNuGetPushSettings settings;
        if (branch == "release") {
            settings = new DotNetNuGetPushSettings {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = EnvironmentVariable("NUGET_KEY")
            };
        } else {
            settings = new DotNetNuGetPushSettings {
                Source = "https://nuget.ellpeck.de/v3/index.json",
                ApiKey = EnvironmentVariable("BAGET_KEY")
            };
        }
        settings.SkipDuplicate = true;
        DotNetNuGetPush("**/MLEM*.nupkg", settings);
});

Task("Document").Does(() => {
    DocFxMetadata("Docs/docfx.json");
    DocFxBuild("Docs/docfx.json");
    if (serve)
        DocFxServe("Docs/_site");
});

Task("Default").IsDependentOn("Pack");
Task("Publish").IsDependentOn("Push");

RunTarget(target);
