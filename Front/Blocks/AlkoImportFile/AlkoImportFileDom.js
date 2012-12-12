var AlkoImportFileDom = Base.extend({
    start: function () {
        this.chooseButton = $("#" + AlkoImportFileConstants.chooseFileId);
        this.chooseAnotherButton = $("#" + AlkoImportFileConstants.chooseAnotherFileId);
        this.input = this.element.find("." + AlkoImportFileConstants.inputClass);
        this.form = $(AlkoImportFileConstants.formSelector);
    },
    registerHandlers: function (control) {
        var self = this;
        var openFileWindow = function (e) {
            self.input.click();
            e.preventDefault();
        };
        this.chooseButton.click(openFileWindow.at(this));
        this.chooseAnotherButton.click(openFileWindow.at(this));
        this.input.change(control.choose.at(control));
    },
    getFileName: function () {
        return this.input.val();
    },
    uploadFile: function () {
        this.form.submit();
    }
});