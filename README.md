# ComputeCS

C# Client Library for Procedural Compute.  

This library contains the core (platform-agnostic) methods for interacting with the Procedural Compute API.  It is intended that this library is imported and "wrapped" by client UI's such as Rhino/Grasshopper and Revit/Dynamo.  This wrapping can be very "thin" (ie. a Grasshopper component can easily/trivially be made that simply wraps the exact inputs and outputs of a particular function in this library.)


## Development notes

Development on this library can be done on any platform (Windows, Linux, Mac, etc) and does not require any special platform-specific libraries other than .NET.

### Setup development environments

#### Setup for Linux development

Install the .NET core libraries for development on Linux from here:
[https://docs.microsoft.com/en-us/dotnet/core/install/linux-package-manager-ubuntu-2004]

```
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1
```


#### Development in Docker Container with Visual Studio Code

The above steps can of course be done within a Docker container to ensure a consistent development environment in the container.  The Dockerfile is contained in this repository and can be built with:

```
docker build . -t ubuntu:dotnet-core-3.1
```

You can develop on Linux by installing Visual Studio Code with the Docker extension to enable remote debugging of code within the Docker container.  Instructions are here: 
[https://code.visualstudio.com/docs/containers/debug-netcore]


### Setup for Testing

#### Secrets in Development

As we are developing with a live API then you will want to keep your username/password secret and NOT publish to this repository.  This can be done in C# with secret management outlined here:
[https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-3.1&tabs=linux]

In summary do the following (on Linux):
```
cd ComputeCS.Tests
dotnet user-secrets set "ComputeAPIUser:Username" "<username>"
dotnet user-secrets set "ComputeAPIUser:Password" "<password>"
dotnet user-secrets set "ComputeAPIUser:Password" "<host>"
```

You can confirm that the secets have been created and saved to a local file path with the following:
```
cat ~/.microsoft/usersecrets/d259b59c-3708-4b6f-a0f9-90d9b8b3560d/secrets.json
```

#### Unit Testing

Unit tests are managed with `NUnit`.
[https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit]

Ensure that your secrets are setup as per the above section.  Then you can run tests with:
```
cd ComputeCS.Tests 
dotnet test
```