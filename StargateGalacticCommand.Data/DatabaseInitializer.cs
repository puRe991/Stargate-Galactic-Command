namespace StargateGalacticCommand.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(GameDbContext context)
        {
            if (context == null)
            {
                return;
            }

            context.Database.EnsureCreated();
        }
    }
}
