namespace RogueSandpit.Models;

public sealed record ThrowTrajectory(
    int ImpactX,
    int ImpactY,
    int LandingX,
    int LandingY,
    BaseNPC Target);
