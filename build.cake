#addin Cake.DocFx&version=0.13.1
#tool docfx.console&version=2.51.0

// this is the upcoming version, for prereleases
var version = Argument("version", "5.0.0");
var target = Argument("target", "Default");
var branch = Argument("branch", "main");
var config = Argument("configuration", "Release");

Task("Prepare").Does(() => {
    DotNetCoreRestore("MLEM.sln");
    
    if (branch != "release") {
        var buildNum = EnvironmentVariable("BUILD_NUMBER");
        if (buildNum != null)
            version += "-" + buildNum;
    }
        
    DeleteFiles("**/*.nupkg");
});

Task("Build").IsDependentOn("Prepare").Does(() =>{
    var settings = new DotNetCoreBuildSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    };
    foreach (var project in GetFiles("**/MLEM*.csproj"))
        DotNetCoreBuild(project.FullPath, settings);
    DotNetCoreBuild("Demos/Demos.csproj", settings);
});

Task("Pack").IsDependentOn("Build").Does(() => {
    var settings = new DotNetCorePackSettings {
        Configuration = config,
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
    };
    foreach (var project in GetFiles("**/MLEM*.csproj"))
        DotNetCorePack(project.FullPath, settings);
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
            Source = "http://localhost:5000/v3/index.json",
            ApiKey = EnvironmentVariable("BAGET")
        };
    }
    settings.SkipDuplicate = true;
    NuGetPush(GetFiles("**/*.nupkg"), settings);
});

Task("Document").Does(() => {
    var path = "Docs/docfx.json";
    DocFxMetadata(path);
    DocFxBuild(path);
});

Task("Default").IsDependentOn("Pack");
Task("Publish").IsDependentOn("Push");

RunTarget(target);