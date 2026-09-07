using MediatR;

namespace VnuaCare.Business.Business.Users.UserCommands;

public class ChangePassUserCommand : IRequest<Unit>
{
    public UpdatePasswordUserModel Model {get; set;}
    
    public ChangePassUserCommand(UpdatePasswordUserModel model)
    {
        Model = model;
    }

    // public class Handler : IRequestHandler<ChangePassUserCommand, Unit>
    // {
    //     private readonly V
    // }
    
    
    
}