var tenant_setting = {
    // Word Clouds
    //    The minimum frequency of a word that will be included in the cloud
    min_freq: 3,
    // A weight factor used to scale the size of the words in the cloud
    weight_factor: 20,
    color: function (word, weight) {
        if (weight < 5) {
            // Words with a weight less than 10 will be displayed in light gray
            return "#AAAAAA";
        } else if (weight < 10) {
            // Words with a weight between 10 and 15 will be displayed in medium gray
            return "red";
        } else {
            // Words with a weight greater than 15 will be displayed in dark gray
            return "green";
        }
    },
    //s
    review_play_speed: 3,
    //responses
    dynamic_input: false,
    dynamic_input_count: 2,
};