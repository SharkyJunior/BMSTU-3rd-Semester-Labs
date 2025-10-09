struct Stack {
    double* arr;
    int len, size;
};

typedef struct Stack Stack;

Stack* initStack(int size) {
    Stack* stack = (Stack*) malloc(sizeof(Stack));
    stack->arr = (double*) malloc(sizeof(double) * size);
    stack->len = 0;
    stack->size = size;
    return stack;
}

void freeStack(Stack* stack) {
    free(stack->arr);
    free(stack);
}

int isEmpty(Stack* stack) {
    return stack->len == 0;
}

int isFull(Stack* stack) {
    return stack->len == stack->size;
}

void push(Stack* stack, double value) {
    if (stack->len == stack->size) {
        stack->arr = (double*) realloc(stack->arr, sizeof(double) * stack->size * 2);
        stack->size *= 2;
    }
    stack->arr[stack->len++] = value;
}

double pop(Stack* stack) {
    return stack->arr[stack->len-- - 1];
}


