namespace Chromia.Postchain.Ft3
{
    public class RateLimitInfo
    {
        public bool IsActive;
        public int MaxPoints;
        public int RecoveryTime;
        public int PointsAtAccountCreation;

        public RateLimitInfo(bool isActive, int maxPoints, int recoveryTime, int pointsAtAccountCreation)
        {
            this.IsActive = isActive;
            this.MaxPoints = maxPoints;
            this.RecoveryTime = recoveryTime;
            this.PointsAtAccountCreation = pointsAtAccountCreation;
        }
    }
}
