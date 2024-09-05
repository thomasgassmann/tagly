# Tagly

Tagly helps you tag photos. Some parts of the project are still a bit unpolished and it specifically targets my workflow.

## Basic setup

There's a gRPC server (`Api`) with an endpoint for uploading photos. Potentially multiple clients use the Avalonia app (`App`) to enter geotags, timestamps and descriptions to photos. Clients upload these photos to the `Api`. Clients authenticate themselves via the import token. Any client with a valid import token can upload photos to the database. There's an exporter (`Exporter`) cli tool which extracts the photos from the sqlite database, and sets the required metadata on the iamges.

## Config files

### Api

- `Jwt:Issuer`: issuer of tokens
- `Jwt:Audience`: audience for tokens
- `Jwt:Key`: secret key used to sign tokens
- `ImportToken`: the token for clients to authenticate themselves to the server
- `User`: username to be used in the JWT token (can be arbitrary)
- `DbPath`: path of the sqlite database

### App

- `DefaultSourcePath`: default path in configuration for source
- `DefaultBackupPath`:  default path in configuration for backup
- `DefaultUrl`: default URL of server
- `DefaultLatitude`: home latitude
- `DefaultLongitude`: home longitude
- `Secure`: if `false` the connection is allowed over plain HTTP, always use `true` in production
