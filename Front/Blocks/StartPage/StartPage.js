var AlkoPage = Page.extend({
    constructor: function (areaName, serverControllerName) {
        this.base(areaName, serverControllerName);
        this.registrator.controlNames.push("AlkoImportFile");
        this.registrator.controlNames.push("DragAndDropUpload");
        this.registrator.controlNames.push("AlkoImportDeclaration");
        this.registrator.controlNames.push("AlkoImportDeclarationLightbox");
    },
    run: function () {
        this.base();
    }
});