# Use the official .NET Framework runtime image (adjust the version as needed)
FROM mcr.microsoft.com/dotnet/framework/runtime:4.7.2

# Set the working directory inside the container
WORKDIR /app

# Copy the published application files into the container
COPY . .

# Expose the port that your WCF service is listening on (for example, 8080)
EXPOSE 8080

# Set the entry point to run your WCF service executable
ENTRYPOINT ["MessangerServer.exe"]
