namespace ATTENDANCE_BE;

public class Location
{
    private static readonly double MAX_DISTANCE = 500;

    private static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }

    public static double Distance(string ori, string des)
    {
        // Check if origin or destination coordinates are empty or null
        if (string.IsNullOrWhiteSpace(ori) || string.IsNullOrWhiteSpace(des))
        {
            throw new ArgumentException("Origin or destination coordinates cannot be empty or null.");
        }

        // Split origin and destination coordinates
        string[] originCoords = ori.Split(',');
        string[] destinationCoords = des.Split(',');

        // Check if coordinates have valid format
        if (originCoords.Length != 2 || destinationCoords.Length != 2)
        {
            throw new ArgumentException("Invalid coordinate format. Coordinates should be in the format 'latitude,longitude'.");
        }

        // Parse latitude and longitude values
        if (!double.TryParse(originCoords[0], out double lat1) || !double.TryParse(originCoords[1], out double lon1) ||
            !double.TryParse(destinationCoords[0], out double lat2) || !double.TryParse(destinationCoords[1], out double lon2))
        {
            throw new ArgumentException("Invalid latitude or longitude value.");
        }

        // Convert latitude and longitude from degrees to radians
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);
        lat1 = ToRadians(lat1);
        lat2 = ToRadians(lat2);

        // Haversine formula to calculate distance
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(lat1) * Math.Cos(lat2) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return 6371000 * c;
    }

    public static bool ApproximatelyCompare(string ori, string des)
    {
        try {
            if (Distance(ori, ori) <= MAX_DISTANCE)
                return true;
        } 
        catch {}

        return false;
    }
}
