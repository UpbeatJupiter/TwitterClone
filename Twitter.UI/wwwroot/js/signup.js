var signup = {
    userDetails: function () {
        let uname = $('#usernameSignup').val();
        let name = $('#nameSignup').val();
        let email = $('#emailSignup').val();
        let password = $('#passwordSignup').val();

        var data = {
            Username: uname,
            Name: name,
            Email: email,
            Password: password
        };

        //$('#registerForm').load("/Home/Register",
        //    {
        //        dto: data
        //    },
        //    function (response) {

        //    }
        //);

        $.ajax({
            url: "/Home/Register",
            type: "POST",
            data: { dto: data },
            success: function (response) {
                if (response.success) {
                    // Redirect to the login page
                    window.location.href = "/Home/Login";
                }
            }
        });
    }
}
