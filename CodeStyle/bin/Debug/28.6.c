#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <math.h>
#include "big_numbers.h"



int multiply(const char* number1, const char* number2, char** result);
int validate_expression(const char *expr);    
int calculate(const char* expr, char **result);


int main(void) {
	
	char* expr = (char*)malloc(501);
	if (!expr) {
		printf("Failed to allocate memory");
		return 3;
	}
	char* result = NULL;
	printf("Podaj wyrazenie zloty panie: ");
	scanf("%500s", expr);
	int s = calculate(expr, &result);
	switch (s) {
		case 0: break;
		case 1: case 2:
			printf("Incorrect input");
			free(expr);
			return 2;
			break;
		case 3:
			printf("Failed to allocate memory");
			free(expr);
			return 3;
			break;
	}
	printf("%s\n", result);
	free(expr);
	free(result);
	return 0;
}

int validate_expression(const char *expr) {
	if (!expr) return 2;
	if (strlen(expr) <= 0) return 1;
	if (*expr != '-' && (*expr > '9' || *expr < '0')) return 1;
	if (*(expr + strlen(expr) - 1) == '+' || *(expr + strlen(expr) - 1) == '-' || *(expr + strlen(expr) - 1) == '*') return 1;
	
	for (int i = 0; *(expr + i) != '\0'; ++i) {
		if ((*(expr + i) > '9' || *(expr + i) < '0') && *(expr + i) != '-' && *(expr + i) != '+' && *(expr + i) != '*') return 1;
	}
	for (int i = 0; *(expr + i) != '\0'; ++i) {
		if (*(expr + i) == '+' || *(expr + i) == '-' || *(expr + i) == '*') {
			if (*(expr + i + 1) != '-' && (*(expr + i + 1) > '9' || *(expr + i + 1) < '0')) return 1;
		}
	}
	
	return 0;
}

int calculate(const char* expr, char **result) {
	if (!expr || !result) return 1;
	if (validate_expression(expr)) return 2;
	char sign;
	const char* find = "*+-";
    const char* kek = NULL;
    const char* ptr = NULL;
    ptr = expr;
    kek = expr;
    if (*expr == '-') ptr++;
    ptr = strpbrk(ptr, find);
    if (!ptr) {
    	*result = (char*)calloc(strlen(expr) + 1, sizeof(char));
    	if (*result == NULL) return 3;
    	memcpy(*result, expr, strlen(expr) + 1);
    	return 0;
	}
	int size = ptr - kek;
	char* buff;
	buff = (char*)calloc(size + 1, sizeof(char));
	if (!buff) return 3;
	char* secondBuff;
	sign = *ptr;
	ptr++;
	memcpy(buff, kek, size);
	while (1) {
		if (*kek == '-') kek++;
		if (*ptr == '-') ptr++;
		kek = strpbrk(kek, find);
		if (!kek) break;
		kek++;
		ptr = strpbrk(ptr, find);
		if (!ptr) {
			int ctrl = 0;
			switch (sign) {
				case '+' :
					ctrl = add(buff, kek, result);
					switch (ctrl) {
						case 0: break;
						case 1: case 2: case 3:
							free(buff);
							free(*result);
							return ctrl;
							break;
					}
					break;
				case '-':
					ctrl = subtract(buff, kek, result);
					switch (ctrl) {
						case 0: break;
						case 1: case 2: case 3:
							free(buff);
							free(*result);
							return ctrl;
							break;
					}
					break;
					
				case '*':
					ctrl = multiply(buff, kek, result);
					switch (ctrl) {
						case 0: break;
						case 1: case 2: case 3:
							free(buff);
							free(*result);
							return ctrl;
							break;
					}
					break;
			}
			break;
		}
		
		ptr++;
		size = ptr - kek;
		if (size <= 0) break;
		secondBuff = (char*)calloc(size + 1, sizeof(char));
		if (!secondBuff) {
			free(buff);
			free(*result);
			return 3;
		}
		int ctrl = 0;
		memcpy(secondBuff, kek, size - 1);
		switch (sign) {
			case '+':
				ctrl = add(buff, secondBuff, result);
				switch (ctrl) {
					case 0: break;
					case 1: case 2: case 3:
						free(buff);
						free(secondBuff);
						
						return ctrl;
						break;
				}
				break;
			case '-':
				ctrl = subtract(buff, secondBuff, result);
				switch (ctrl) {
					case 0: break;
					case 1: case 2: case 3:
						free(buff);
						free(secondBuff);
						return ctrl;
						break;
				}
				break;
			case '*':
				ctrl = multiply(buff, secondBuff, result);
				switch (ctrl) {
					case 0: break;
					case 1: case 2: case 3:
						free(buff);
						free(secondBuff);
						return ctrl;
						break;
				}
				break;
		}
		free(buff);
		buff = *result;
		free(secondBuff);
		sign = *(ptr - 1);
	}
	free(buff);
	return 0;
}

int multiply(const char* number1, const char* number2, char** result) {
	if (!number1 || !number2 || !result) return 1;
	if (strlen(number1) <= 0 || strlen(number2) <= 0) return 2;
	if (validate(number1) || validate(number2)) return 2;
	if (*number1 == '0' || *number2 == '0') {
		*result = (char*)calloc(2, sizeof(char));
		if (*result == NULL) return 3;
		**result = '0';
		return 0;
	}
	if (*number1 == '-' && *number2 != '-') {
		int ctrl = multiply(number1 + 1, number2, result);
		if (!ctrl) {
			reverse(*result);
			*(*result + strlen(*result)) = '-';
			reverse(*result); 
		}
 		return ctrl;
	}
	if (*number2 == '-' && *number1 != '-') {
		int ctrl = multiply(number1, number2 + 1, result);
		if (!ctrl) {
			reverse(*result);
			*(*result + strlen(*result)) = '-';
			reverse(*result); 
		}
		return ctrl;
	}
	if (*number1 == '-' && *number2 == '-') {
		int ctrl = multiply(number1 + 1, number2 + 1, result);
		return ctrl;
	}
	if (*number1 == 1 && strlen(number1) == 1) {
		*result = calloc(strlen(number2) + 1, sizeof(char));
		if (*result == NULL) return 3;
		memcpy(*result, number2, strlen(number2) + 1);
		return 0;
	}
	if (*number2 == 1 && strlen(number2) == 1) {
		*result = calloc(strlen(number1) + 1, sizeof(char));
		if (*result == NULL) return 3;
		memcpy(*result, number1, strlen(number1) + 1);
		return 0;
	} 
	*result = (char*)calloc(strlen(number1) * strlen(number2) + 50, sizeof(char));
	if (*result == NULL) return 3;
	struct big_numbers_t *res = (struct big_numbers_t*)malloc(sizeof(struct big_numbers_t));
	if (!res) {
		free(*result);
		return 3;
	}
	res->capacity = strlen(number1) * strlen(number2);
	res->size = 0;
	res->big_number = (char**)calloc(strlen(number1) * strlen(number2), sizeof(char*));
	if (!res->big_number) {
		free(*result);
		destroy_big_numbers(&res);
		return 3;
	}
	int k = 0, mem = 0;
	for (int i = strlen(number2) - 1, x = 0; i >= 0; --i, ++x, k = 0, mem = 0) {
		for (k = 0; k < x; ++k) {
			*(*result + k) = '0';
		}
		for (int j = strlen(number1) - 1; j >= 0; --j) {
			int temp = (*(number2 + i) - '0') * (*(number1 + j) - '0') + mem;
			mem = temp / 10;
			*(*result + k++) = (temp % 10) + '0';
		}
		if (mem) *(*result + k) = mem + '0';
		reverse(*result);
		if (add_big_number(res, *result) == 2) {
			free(*result);
			destroy_big_numbers(&res);
			return 3;
		}
	}
	char *temp = total_sum_big_numbers(res);
	if (!temp) {
		destroy_big_numbers(&res);
		free(*result);
		return 3;
	}
	memcpy(*result, temp, strlen(temp) + 1);
	free(temp);
	destroy_big_numbers(&res);
	return 0;
}