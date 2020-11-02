$(document).ready(function () {
  $('#multiselect').multiselect({
    buttonWidth: '160px',
    includeSelectAllOption: true,
    nonSelectedText: 'Select an Option'
  });
});

function getSelectedValues() {
  var selectedVal = $("#multiselect").val();
  for (var i = 0; i < selectedVal.length; i++) {
    function innerFunc(i) {
      setTimeout(function () {
        location.href = selectedVal[i];
      }, i * 2000);
    }
    innerFunc(i);
  }
}

