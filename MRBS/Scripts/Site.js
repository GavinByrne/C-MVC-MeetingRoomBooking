//Site wide scripts

jQuery.validator.methods.date = function (value, element) {
    return this.optional(element) || Globalize.parseDate(value, "d/M/y", "en");
}

