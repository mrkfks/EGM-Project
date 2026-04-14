# Code Citations

## License: bilinmiyor

[GitHub Repository](https://github.com/vladyslav-kozh/OATHJWT/tree/4a2cafac80cf9b1e08351cb20df81dd3a6841684/oathjwt/Startup.cs)

```plaintext
TokenValidationParameters = new TokenValidationParameters
  {
      ValidateIssuer = true,
      ValidateAudience = true,
      ValidateLifetime = true,
      ValidateIssuerSigningKey = true,
      ValidIssuer = "your-issuer",
      ValidAudience = "your-audience",
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-secret-key"))
  }
```

