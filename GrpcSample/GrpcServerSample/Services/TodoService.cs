using System.Threading.Tasks;
using Grpc.Core;
using Todo.V1;

namespace GrpcServerSample.Services
{
    public class TodoService : Todo.V1.Todo.TodoBase
    {
        public override Task<TodoListResponse> TodoList(TodoRequest request, ServerCallContext context)
        {
            return Task.FromResult(
                new TodoListResponse()
                {
                    Items =
                    {
                        new TodoItem()
                        {
                            Title = "test",
                            Description = "gRPC test"
                        }
                    }
                }
            );
        }
    }
}
