var login = {
    userLogin: function () {
        let uname = $('#usernameLogin').val();
        let password = $('#passwordLogin').val();

        var data = {
            Username: uname,
            Password: password
        };


        $.ajax({
            type: "POST",
            url: "/Home/Login", // Login action'ının URL'sini güncelleyin.
            data: { dto: data },
            success: function (response) {
                // Başarılı cevap geldiyse
                window.location.href = "/Home/HomePage";
                //yönlendirme 
            },
            error: function () {
                // Başarısız cevap geldiyse
                alert("Login failed. Invalid username or password.");
            }
            
        });
    }
}
