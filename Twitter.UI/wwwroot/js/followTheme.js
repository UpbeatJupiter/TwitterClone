var followTheme = {

    followUnfollow: function (userid, myUserId) {
        let button = document.getElementById(userid);
        button.classList.toggle("followed");

        //takip edilmiyorsa takip et
        if (button.innerText == 'Follow') {
            button.innerText = 'Unfollow';
           

            $.post("/Home/FollowUser",
                {
                    userid: userid,
                    myUserId: myUserId
                },
                function (response) {
                    followTheme.getAllTweets(myUserId);

                }
            );

        }
        //takip ediliyorsa takipten çık 
        else {
            button.innerText = 'Follow'
            button.className = "follow-button";

            $.post("/Home/UnfollowUser",
                {
                    userid: userid,
                    myUserId: myUserId
                },
                function (response) {
                    followTheme.getAllTweets(myUserId);


                }
            );
        }
    },

    //kullanıcıların tweetlerini getirir
    getAllTweets: function (myUserId) {
        $('#allTweets').load("/Home/AllTweets",
            {
                myUserId: myUserId
            },
            function (response) {


            }
        );
    }
}