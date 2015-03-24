(function(global, $) {
    $(function() {
        $('#load-more')
            .click(function() {
                var lastIteratorContext = global.lastIteratorContext;
                $.post("/Tasks/Scroll", { iteratorContext: lastIteratorContext },
                    function(data) {
                        var nextResult = $(data);
                        console.log(nextResult.find("tr"));
                        $('#result-container').append(nextResult.find("tr"));
                        global.lastIteratorContext = nextResult.data('next-iterator-context');
                    });
            });
    });


    $(function() {

        $(".cancel-task-button")
            .click(function() {
                var button = $(this);

                if (confirm("Отменить задачу?")) {
                    button.addClass("m-progress");
                    button.prop('disabled', true);

                    $.post(button.data('url'))
                        .done(function() {
                            alert('Задача успешно отменена.');
                            button.remove();
                        })
                        .fail(function() {
                            alert('Не удалось отменить задачу.');
                            button.removeClass("m-progress");
                            button.prop('disabled', false);
                        });
                }
            });

        $(".rerun-task-button")
            .click(function() {
                var button = $(this);

                if (confirm("Перезапустить задачу?")) {
                    button.addClass("m-progress");
                    button.prop('disabled', true);

                    $.post(button.data('url'))
                        .done(function() {
                            alert('Задача успешно перезапущена.');
                            button.remove();
                        })
                        .fail(function() {
                            alert('Не удалось перезапустить задачу.');
                            button.removeClass("m-progress");
                            button.prop('disabled', false);
                        });
                }
            });

    });

})(window, jQuery);

