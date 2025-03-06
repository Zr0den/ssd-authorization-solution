# Authorization start

Starter project for authorization workshop.

## Getting started

```sh
docker compose up --build
```

<http://localhost:5171/swagger/index.html>

Set `useCookies` and `useSessionCookies` to true for login.


### Changes we have made
We define authorization policies in Program.cs
We have updated our Program.cs to include settings for policies (Authorization) 
We also implemented a bit of AspNetCore.Identity, JWT tokens & Cookies in Program.cs as well (Authentication)
As part of our Identity policy we also enforce stronger passwords as it supports the "Secure by Default" principle
We also do some minor things like app.UseExceptionHandler("/error"); to redirect errors to a generic page and app.UseHsts(); to enforce HTTPS Strict Transport Security

We have updated our Controllers to enforce the Authorization policies

Since Linq is used for our database queries (which effectively parameterizes them behind the scenes), we did not feel like we strictly speaking needed to do much with inputs
Perhaps we should have done something more with error/exception handling, but oh well

Interact with the program through swagger (adress above) - We struggled a lot with setting up an environment where we could properly test the policies as if we were different users
But trying to do something that requires a policy "correctly" gives a 403 forbidden. Should work with postman though. :)

Our excuse is that we started too late (our fault) and have run out of time. We are going to a conference in Bangkok tomorrow and it is late. Sorry. (Who asked?)