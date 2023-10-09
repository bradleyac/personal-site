aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin 693087155287.dkr.ecr.us-east-1.amazonaws.com;
dotnet publish --os linux --arch x64 /t:PublishContainer -c Release;
aws ecs update-service --cluster ecs-tutorial --service ecs-tutorial-service2 --force-new-deployment