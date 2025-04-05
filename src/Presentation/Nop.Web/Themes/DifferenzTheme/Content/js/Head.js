let loader = {
	active: $(".loading").addClass("active"),
	inactive: $(".loading").removeClass("active")
};

$("form").submit((function (a) {
	loader.active;
}));
function displayAjaxLoading(display) {
	if (display) {
		loader.active;
	}
	else {
		loader.inactive;
	}
}