export const env = {
  port: Number(process.env.PORT ?? 3000),
  databaseUrl: process.env.DATABASE_URL ?? "postgres://farm:farm@localhost:5432/farm_life",
  jwtSecret: process.env.JWT_SECRET ?? "dev-secret-change-in-production",
};
