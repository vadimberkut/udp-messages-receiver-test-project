

# General

Test project on ASP.NET 5, EF Core.


## Projects

MessageReceiver - UDP message receiver with HTTP API.

MessageSender - console app that sends test UDP messages.


# Run locally

## Windows
```bash
# Receiver
./cli/run-MessageReceiver-Development.cmd

# Sender
./cli/run-MessageSender-Development.cmd
```

Receiver API Swagger

http://localhost:5040/swagger

# Heroku

Urls:

https://udp-messages-receiver-test-prj.herokuapp.com/
https://udp-messages-receiver-test-prj.herokuapp.com/swagger/index.html


NB: on Heroku only http requests are accepted so you can't expose any different port than $PORT. This means that additional UDP port can't be exposed.
