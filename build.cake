#addin nuget:?package=Cake.DocFx&version=1.0.0
#tool dotnet:?package=docfx&version=2.78.3

// this is the upcoming version, for prereleases
var version = Argument("version", "8.0.1");
var target = Argument("target", "Default");
var gitRef = Argument("ref", "refs/heads/main");
var buildNum = Argument("buildNum", "");
var config = Argument("configuration", "Release");
var serve = HasArgument("serve");

Task("Prepare").Does(() => {
    DotNetWorkloadInstall("android");

    DotNetRestore("MLEM.sln");
    DotNetRestore("MLEM.FNA.sln");
    DotNetRestore("MLEM.KNI.sln");

    if (!gitRef.StartsWith("refs/tags/") && !string.IsNullOrEmpty(buildNum)) {
        Information($"Appending {buildNum} to version");
        version += $"-ci.{buildNum}";
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
    DotNetBuild("MLEM.KNI.sln", settings);
});

Task("Test").IsDependentOn("Prepare").Does(() => {
    var settings = new DotNetTestSettings {
        Configuration = config,
        Collectors = {"XPlat Code Coverage"},
        Loggers = {"console;verbosity=normal", "nunit"}
    };
    DotNetTest("MLEM.sln", settings);
    DotNetTest("MLEM.FNA.sln", settings);
    DotNetTest("MLEM.KNI.sln", settings);
});

Task("Pack").IsDependentOn("Prepare").Does(() => {
    var settings = new DotNetPackSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    };
    DotNetPack("MLEM.sln", settings);
    DotNetPack("MLEM.FNA.sln", settings);
    DotNetPack("MLEM.KNI.sln", settings);
});

Task("Push").WithCriteria(gitRef == "refs/heads/main" || gitRef.StartsWith("refs/tags/"), "Not on main branch or tag").IsDependentOn("Pack").Does(() => {
    DotNetNuGetPushSettings settings;
    if (gitRef.StartsWith("refs/tags/")) {
        Information("Pushing to public feed");
        settings = new DotNetNuGetPushSettings {
            Source = "https://api.nuget.org/v3/index.json",
            ApiKey = EnvironmentVariable("NUGET_KEY")
        };
    } else {
        Information("Pushing to private feed");
        settings = new DotNetNuGetPushSettings {
            Source = "https://nuget.ellpeck.de/v3/index.json",
            ApiKey = EnvironmentVariable("BAGET_KEY")
        };
    }
    settings.SkipDuplicate = true;
    DotNetNuGetPush("**/MLEM*.nupkg", settings);
});

Task("Document").IsDependentOn("Prepare").Does(() => {
    DocFxMetadata("Docs/docfx.json");
    DocFxBuild("Docs/docfx.json");
    if (serve)
        DocFxServe("Docs/_site");
});

Task("PublishWeb").IsDependentOn("Prepare").Does(() => {
    DotNetPublish("Demos.Web/Demos.Web.KNI.csproj", new DotNetPublishSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    });
});

Task("Default").IsDependentOn("Build").IsDependentOn("Test").IsDependentOn("Pack");
Task("Publish").IsDependentOn("Push");

RunTarget(target);
