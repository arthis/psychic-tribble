FROM ubuntu:16.10

RUN apt-get update && apt-get install -y apt-transport-https curl dirmngr

# dotnet core 
RUN echo "deb [arch=amd64] https://apt-mo.trafficmanager.net/repos/dotnet-release/ yakkety main" > /etc/apt/sources.list.d/dotnetdev.list 
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 417A0893
RUN apt-get update && apt-get install -y dotnet-dev-1.0.0-preview2.1-003177


# mssql tools
RUN curl https://packages.microsoft.com/keys/microsoft.asc |  apt-key add - \
    && curl https://packages.microsoft.com/config/ubuntu/16.04/prod.list | tee /etc/apt/sources.list.d/msprod.list

RUN apt-get update -y \

    && ACCEPT_EULA=Y apt-get install -y odbcinst1debian2-utf16 \
    unixodbc-dev-utf16 \
    mssql-tools 
RUN ln -sfn /opt/mssql-tools/bin/sqlcmd-13.0.1.0 /usr/bin/sqlcmd \
    && ln -sfn /opt/mssql-tools/bin/bcp-13.0.1.0 /usr/bin/bcp

# # docker images
# RUN apt-get install -y docker 
# RUN docker pull microsoft/mssql-server-linux \
#     && docker pull rabbitmq \
#     && docker pull eventstore/eventstore

# RUN docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d microsoft/mssql-server-linux \
#     && docker run -d --hostname my-rabbit  -p 8080:15672 rabbitmq:3-management \
#     && docker  run -d -it -p 2113:2113 -p 1113:1113 eventstore/eventstore

# copy and build application

#Install Mono to run fake
RUN apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF 
RUN echo "deb http://download.mono-project.com/repo/debian wheezy main" | tee /etc/apt/sources.list.d/mono-xamarin.list 
RUN apt-get update -y \
    && apt-get install -y mono-complete

COPY . /

RUN ./build.sh