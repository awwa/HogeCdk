#!/bin/bash
yum update -y
# Dotnet 6 setup
rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
yum install -y dotnet-sdk-6.0
sudo -u ec2-user dotnet tool install --global dotnet-ef
# PostgreSQL setup
amazon-linux-extras install postgresql14
