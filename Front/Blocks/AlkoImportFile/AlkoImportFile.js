var AlkoImportFile = Element.extend({
    constructor: function (element) {
        this.base(element);
        this.dom = this.dom.extend(new AlkoImportFileDom());
        this.dom.start();
        this.dom.registerHandlers(this);
    },
    choose: function () {
        this.dom.uploadFile();
    }
}, {
    register: function (element) {
        if (element.hasClass(AlkoImportFileConstants.importClass))
            return new AlkoImportFile(element);
    }
});