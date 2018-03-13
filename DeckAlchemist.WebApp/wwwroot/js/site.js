﻿window.onload = function() {
    "use strict";

    // Options for Message
    //----------------------------------------------
    var options = {
        'btn-loading': '<i class="fa fa-spinner fa-pulse"></i>',
        'btn-success': '<i class="fa fa-check"></i>',
        'btn-error': '<i class="fa fa-remove"></i>',
        'msg-success': 'All Good! Redirecting...',
        'msg-error': 'Wrong login credentials!',
        'useAJAX': true,
    };

    // Login Form
    //----------------------------------------------
    // Validation
    /*$("#login-form").validate({
        rules: {
            lg_username: "required",
            lg_password: "required",
        },
        errorClass: "form-invalid"
    });*/

    // Form Submission
    $("#login-form").submit(function(e) {
        e.preventDefault();

        remove_loading($(this));
        login($(this));
        return false;
    });

    // Register Form
    //----------------------------------------------
    // Validation
    /*
    $("#register-form").validate({
        rules: {
            reg_username: "required",
            reg_password: {
                required: true,
                minlength: 5
            },
            reg_password_confirm: {
                required: true,
                minlength: 5,
                equalTo: "#register-form [name=reg_password]"
            },
            reg_email: {
                required: true,
                email: true
            },
            reg_agree: "required",
        },
        errorClass: "form-invalid",
        errorPlacement: function( label, element ) {
            if( element.attr( "type" ) === "checkbox" || element.attr( "type" ) === "radio" ) {
                element.parent().append( label ); // this would append the label after all your checkboxes/labels (so the error-label will be the last element in <div class="controls"> )
            }
            else {
                label.insertAfter( element ); // standard behaviour
            }
        }
    });
    */
    // Form Submission
    $("#register-form").submit(function() {
        e.preventDefault();

        remove_loading($(this));
        register($(this));
        return false;
    });

    // Forgot Password Form
    //----------------------------------------------
    // Validation
    /*
    $("#forgot-password-form").validate({
        rules: {
            fp_email: "required",
        },
        errorClass: "form-invalid"
    });
    */
    // Form Submission
    $("#forgot-password-form").submit(function() {
        remove_loading($(this));

        if(options['useAJAX'] == true)
        {
            // Dummy AJAX request (Replace this with your AJAX code)
            // If you don't want to use AJAX, remove this
            dummy_submit_form($(this));

            // Cancel the normal submission.
            // If you don't want to use AJAX, remove this
            return false;
        }
    });

    // Loading
    //----------------------------------------------
    function remove_loading($form)
    {
        $form.find('[type=submit]').removeClass('error success');
        $form.find('.login-form-main-message').removeClass('show error success').html('');
    }

    function form_loading($form)
    {
        $form.find('[type=submit]').addClass('clicked').html(options['btn-loading']);
    }

    function form_success($form)
    {
        $form.find('[type=submit]').addClass('success').html(options['btn-success']);
        $form.find('.login-form-main-message').addClass('show success').html(options['msg-success']);

        //document.location.href = "decks.html";
    }

    function form_failed($form, msg)
    {
        $form.find('[type=submit]').addClass('error').html(options['btn-error']);
        $form.find('.login-form-main-message').addClass('show error').html(msg);
    }

    // Dummy Submit Form (Remove this)
    //----------------------------------------------
    // This is just a dummy form submission. You should use your AJAX function or remove this function if you are not using AJAX.
    function login($form)
    {
        //if($form.valid())
        //{
            //form_loading($form);

            var email = $('#username').val();
            var password = $('#password').val();

            firebase.auth().signInWithEmailAndPassword(email, password)
                .then(function() {
                    firebase.auth().currentUser.getIdToken(true).then(function(idToken) {
                        fetch("http://localhost:5000/api/login", {
                            headers: {
                                'Authorization': "Bearer "+idToken
                            }
                        })
                    }).then(function(){
                        alert("Done")
                    })
                    //form_success($form);
                })
                .catch(function(error) {
                    // Handle Errors here.
                    var errorCode = error.code;
                    var errorMessage = error.message;

                    form_failed($form, errorMessage);
                });
        //}
    }

    function register($form) {
        if($form.valid())
        {
            form_loading($form);

            var email = $('#reg_username').val();
            var password = $('#reg_password').val();

            firebase.auth().createUserWithEmailAndPassword(email, password)
                .then(function() {
                    form_success($form);
                })
                .catch(function(error) {
                    // Handle Errors here.
                    var errorCode = error.code;
                    var errorMessage = error.message;

                    form_failed($form, errorMessage);
                });
        }
    }
}
