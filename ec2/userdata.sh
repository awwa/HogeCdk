#!/bin/bash
yum update -y
# Dotnet 6 setup
rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
yum install -y dotnet-sdk-6.0
sudo -u ec2-user dotnet tool install --global dotnet-ef
# PostgreSQL setup
amazon-linux-extras install postgresql14

# "# download quid deploy script",
# {
#     "Fn::Sub": [
#         "aws s3 cp s3://${Service}-artifacts/cf/deploy-quid.sh /home/ec2-user/",
#         {
#             "Service": {
#                 "Ref": "Service"
#             }
#         }
#     ]
# },
# "chmod +x /home/ec2-user/deploy-quid.sh",
# "chown ec2-user:ec2-user /home/ec2-user/deploy-quid.sh",
# {
#     "Fn::Sub": [
#         "aws s3 cp s3://${Service}-artifacts/cf/deploy-quiet.sh /home/ec2-user/",
#         {
#             "Service": {
#                 "Ref": "Service"
#             }
#         }
#     ]
# },
# "chmod +x /home/ec2-user/deploy-quiet.sh",
# "chown ec2-user:ec2-user /home/ec2-user/deploy-quiet.sh",
# "# download quid service configuration",
# {
#     "Fn::Sub": [
#         "aws s3 cp s3://${Service}-artifacts/cf/quid.service /etc/systemd/system/",
#         {
#             "Service": {
#                 "Ref": "Service"
#             }
#         }
#     ]
# },
# "# Prepare for GDAL install",
# "yum -y update",
# "yum groupinstall -y \"development tools\"",
# "yum install -y openssl-devel libffi-devel bzip2-devel",
# "yum install -y libuuid-devel ncurses-devel readline-devel",
# "yum install -y xz-devel zlib-devel",
# "yum install -y sqlite-devel",
# "amazon-linux-extras install epel -y",
# "yum remove -y openssl openssl-devel",
# "yum install -y openssl11 openssl11-devel",
# "# Install PROJ Library",
# "cd ~",
# "wget http://download.osgeo.org/proj/proj-7.2.1.tar.gz",
# "tar xzf proj-7.2.1.tar.gz",
# "cd proj-7.2.1",
# "./configure",
# "make",
# "sudo make install",
# "# Install GDAL",
# "cd ~",
# "wget http://download.osgeo.org/gdal/3.3.1/gdal-3.3.1.tar.gz",
# "tar xzf gdal-3.3.1.tar.gz",
# "cd gdal-3.3.1",
# "./configure",
# "make",
# "sudo make install"
