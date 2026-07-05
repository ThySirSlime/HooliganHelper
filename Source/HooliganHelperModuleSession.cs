using Celeste.Mod.HooliganHelper.Entities;

namespace Celeste.Mod.HooliganHelper;

public class HooliganHelperModuleSession : EverestModuleSession
{
    //Bumper Refill
    public int BumperDashes {get; set;}
    public bool DashingWithBumper {get; set;}
    public BumperRefill LastBumperCollected {get; set;}
    
    //Bough Refill
    public int BoughDashes {get; set;}
    public bool DashingWithBough {get; set;}
    
    //Metamorphosis Refill
    public int MetamorphosisDashes {get; set;}
    public bool DashingWithMetamorphosis {get; set;}
    
    //Daisy Refill
    public int DaisyDashes {get; set;}
    public bool DashingWithDaisy {get; set;}
    
    //Knife Refill
    public int KnifeDashes {get; set;}
    public bool DashingWithKnife {get; set;}
}