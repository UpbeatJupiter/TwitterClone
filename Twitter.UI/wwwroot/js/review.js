var review = {
    userReview: function (id) {
        let review = $('#subject').val();


        $('#reviewInput').load("/Home/SendToSheets",
            {
                userid: id,
                review: review
            },
            function (response) {

            }
        );
    }
}
