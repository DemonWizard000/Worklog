var tenant_setting = {
  min_freq: 5,
  weight_factor: 20,
  color: function (word, weight) {
    if (weight < 10) {
      return "#AAAAAA";
    } else if (weight < 15) {
      return "#888888";
    } else {
      return "#333333";
    }
  },
  dynamic_input: false,
  dynamic_input_count: 5,
};