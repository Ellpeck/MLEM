#addin Cake.DocFx&version=1.0.0
#tool docfx.console&version=2.59.4

// this is the upcoming version, for prereleases
var version = Argument("version", "6.2.0");
var target = Argument("target", "Default");
var branch = Argument("branch", "main");
var config = Argument("configuration", "Release");

Task("Prepare").Does(() => {
    DotNetRestore("MLEM.sln");
    DotNetRestore("MLEM.FNA.sln");

    if (branch != "release") {
        var buildNum = EnvironmentVariable("BUILD_NUMBER");
        if (buildNum != null)
            version += "-" + buildNum;
    }

    DeleteFiles("**/MLEM*.nupkg");
});

Task("Build").IsDependentOn("Prepare").Does(() =>{
    var settings = new DotNetBuildSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    };
    DotNetBuild("MLEM.sln", settings);
    DotNetBuild("MLEM.FNA.sln", settings);
});

Task("Test").IsDependentOn("Build").Does(() => {
    var settings = new DotNetTestSettings {
        Configuration = config,
        Collectors = {"XPlat Code Coverage"}
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

Task("Push").WithCriteria(branch == "main" || branch == "release").IsDependentOn("Pack").Does(() => {
    NuGetPushSettings settings;
    if (branch == "release") {
        settings = new NuGetPushSettings {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = EnvironmentVariable("NUGET")
        };
    } else {
        settings = new NuGetPushSettings {
            Source = "https://nuget.ellpeck.de/v3/index.json",
            ApiKey = EnvironmentVariable("BAGET")
        };
    }
    settings.SkipDuplicate = true;
    NuGetPush(GetFiles("**/MLEM*.nupkg"), settings);
});

Task("Document").Does(() => {
    var path = "Docs/docfx.json";
    DocFxMetadata(path);
    DocFxBuild(path);
});

Task("Default").IsDependentOn("Pack");
Task("Publish").IsDependentOn("Push");

RunTarget(target);
