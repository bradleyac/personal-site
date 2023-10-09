# You can also pull these images from DockerHub amazon/aws-lambda-dotnet:7
FROM mcr.microsoft.com/dotnet/runtime:7.0

# Set the image's internal work directory
WORKDIR /var/task
  
# Copy function code
COPY "bin/Release/net7.0/linux-x64"  .
  
# Set the entrypoint to the bootstrap
ENTRYPOINT ["/usr/bin/dotnet", "exec", "/var/task/bootstrap.dll"]