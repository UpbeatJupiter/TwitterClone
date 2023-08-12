var login = {
    userLogin: function () {
        let uname = $('#usernameLogin').val();
        let password = $('#passwordLogin').val();

        var data = {
            Username: uname,
            Password: password
        };


        $('#LoginForm').load("/Home/Login",
            {
                dto: data
            },
            function (response) {
            }
        );
    }
}
