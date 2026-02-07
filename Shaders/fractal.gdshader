// Godot 4 Fragment Shader for Division by Zero Julia Fractal
// 
// A rational Julia set that generates division by 0 as a win condition.
// Uses the formula: z = 1 / (z² - shift)
// 
// The game mechanic: Find parameters where division by 0 occurs during iteration!

shader_type canvas_item;

// Fractal parameters
uniform float center_x = 0.0;
uniform float center_y = 0.0;
uniform float zoom = 3.0;
uniform int max_iterations = 100;
uniform float color_range = 1.0;
uniform float color_shift = 0.0;

// Division by Zero Julia set parameters
// Adjust these to find where division by 0 occurs!
uniform float shift_real = -0.4;
uniform float shift_imag = 0.6;

// ============================================================================
// Complex Number Operations
// ============================================================================

// Represents a complex number as a vec2 (real, imaginary)
vec2 complex_add(vec2 a, vec2 b) {
	return a + b;
}

vec2 complex_subtract(vec2 a, vec2 b) {
	return a - b;
}

vec2 complex_multiply(vec2 a, vec2 b) {
	return vec2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}

vec2 complex_divide(vec2 a, vec2 b) {
	float modulus_squared = dot(b, b);
	if (modulus_squared == 0.0) return vec2(0.0);
	return vec2((a.x * b.x + a.y * b.y) / modulus_squared,
				(a.y * b.x - a.x * b.y) / modulus_squared);
}

vec2 complex_square(vec2 z) {
	return vec2(z.x * z.x - z.y * z.y, 2.0 * z.x * z.y);
}

float complex_modulus_squared(vec2 z) {
	return dot(z, z);
}

float complex_modulus(vec2 z) {
	return length(z);
}

// ============================================================================
// Fractal Algorithms
// ============================================================================

// Division by Zero Julia set iteration
// Formula: z = 1 / (z² - shift)
// Division by 0 occurs when z² = shift (THE WIN CONDITION!)
int compute_division_julia(vec2 z, vec2 shift, out float smooth_value, out bool div_by_zero) {
	int iterations = 0;
	float max_modulus_sq = 10000.0; // Large escape radius for rational function
	smooth_value = 0.0;
	div_by_zero = false;
	float division_threshold = 1e-6; // Threshold to detect near-division
	
	while (complex_modulus_squared(z) < max_modulus_sq && iterations < max_iterations) {
		// Compute z² - shift (the denominator)
		vec2 z_squared = complex_square(z);
		vec2 denominator = complex_subtract(z_squared, shift);
		float denominator_modulus = complex_modulus(denominator);
		
		// Check for division by 0
		if (denominator_modulus < division_threshold) {
			div_by_zero = true;
			smooth_value = float(max_iterations) + 10.0; // Special value to mark as win
			return iterations;
		}
		
		// z = 1 / (z² - shift)
		z = complex_divide(vec2(1.0, 0.0), denominator);
		iterations++;
	}
	
	// Smooth iteration value using continuous escape time
	if (iterations < max_iterations) {
		float modulus = complex_modulus(z);
		if (modulus > 0.0) {
			smooth_value = float(iterations) + 1.0 - log(log(modulus)) / log(2.0);
		} else {
			smooth_value = float(iterations);
		}
	} else {
		smooth_value = float(max_iterations);
	}
	
	return iterations;
}

// ============================================================================
// Color Generation
// ============================================================================

// Convert HSV to RGB
vec3 hsv_to_rgb(vec3 hsv) {
	float h = fract(hsv.x) * 6.0;
	float s = hsv.y;
	float v = hsv.z;
	
	vec3 rgb = v * mix(vec3(1.0), clamp(abs(fract(h + vec3(3.0, 2.0, 1.0)) * 6.0 - 3.0) - 1.0, 0.0, 1.0), s);
	return rgb;
}

// Generate color from iteration count using smooth coloring
// Special handling for division by 0 (the win condition!)
vec4 get_color(int iterations, float smooth_value, bool div_by_zero) {
	// Division by 0 - WIN! Show bright red
	if (div_by_zero) {
		return vec4(1.0, 0.0, 0.0, 1.0); // Bright red for win condition
	}
	
	if (iterations == max_iterations) {
		return vec4(0.0, 0.0, 0.0, 1.0); // Black for in-set points
	}
	
	// Normalize smooth value
	float normalized = mod(smooth_value / float(max_iterations), 1.0);
	
	// Apply color shift
	float hue = mod(normalized + color_shift, 1.0);
	
	// Convert HSV to RGB
	vec3 rgb = hsv_to_rgb(vec3(hue, 1.0, 1.0));
	
	return vec4(rgb, 1.0);
}

// ============================================================================
// Main Fragment Shader
// ============================================================================

void fragment() {
	// Convert fragment coordinates to normalized coordinates [0, 1]
	vec2 uv = UV;
	
	// Calculate viewport dimensions in complex plane
	float width = zoom;
	float height = zoom * (float(SCREEN_PIXEL_SIZE.y) / float(SCREEN_PIXEL_SIZE.x));
	
	// Convert screen coordinates to complex plane coordinates
	vec2 complex_pos = vec2(
		uv.x * width + center_x - width / 2.0,
		(1.0 - uv.y) * height + center_y - height / 2.0
	);
	
	// Compute division by zero Julia set
	int iterations = 0;
	float smooth_value = 0.0;
	bool div_by_zero = false;
	
	vec2 shift = vec2(shift_real, shift_imag);
	iterations = compute_division_julia(complex_pos, shift, smooth_value, div_by_zero);
	
	// Convert iterations to color (includes special handling for division by 0)
	COLOR = get_color(iterations, smooth_value, div_by_zero);
}
