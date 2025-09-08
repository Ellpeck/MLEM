exports.preTransform = function (model) {
    if (model._path.includes("index")) {
        // reduce header levels by 1 to allow for TOC navigation
        for (let i = 5; i >= 1; i--)
            model.conceptual = model.conceptual.replaceAll(`<h${i}`, `<h${i + 1}`).replaceAll(`</h${i}`, `</h${i + 1}`);
    }
    return model;
};
