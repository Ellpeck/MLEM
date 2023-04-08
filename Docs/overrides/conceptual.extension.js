exports.preTransform = function (model) {
    // point to the release branch in the readme when on the website
    if (model._path.includes("index"))
        model.conceptual = model.conceptual.replaceAll(/\/MLEM(\/[^/]+)?\/main\//g, "/MLEM$1/release/");
    return model;
};
