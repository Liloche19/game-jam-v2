// Godot 4 Fragment Shader for Division by Zero Julia Fractal
// 
// A rational Julia set that generates division by 0 as a win condition.
// Uses the formula: z = (k / (z - v))^2 + c
// 
// The game mechanic: Find the v parameter where division by 0 occurs!
// Division by zero happens when z equals v during iteration.

shader_type spatial;
render_mode unshaded;

// Fractal parameters
uniform float center_x = 0.0;
uniform float center_y = 0.0;
uniform float zoom = 3.0;
uniform int max_iterations = 100;
uniform float color_range = 1.0;
uniform float color_shift = 0.0;
uniform float tex_w = 800.0;
uniform float tex_h = 600.0;

// Division by Zero Julia set parameters
// v is the SINGULARITY (player-controlled) - where division by zero occurs!
// c is a fixed attractor constant
// k is a scale factor
uniform float v_real = -0.4;
uniform float v_imag = 0.6;
uniform float c_real = 0.15;
uniform float c_imag = -0.2;
uniform float k_value = 1.0;

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
// Formula: z = (k / (z - v))^2 + x
// Division by 0 occurs when z = v (THE WIN CONDITION!)
// Returns negative value if division by zero detected
int compute_division_julia(vec2 z, vec2 v, vec2 x, float k, out float smooth_value) {
	int iterations = 0;
	float max_modulus_sq = 10000.0; // Large escape radius for rational function
	smooth_value = 0.0;
	float division_threshold = 0.5e-2f; // Balanced threshold for GPU precision

	while (complex_modulus_squared(z) < max_modulus_sq && iterations < max_iterations) {
		// Compute z - v (the denominator - v is player-controlled singularity!)
		vec2 denominator = complex_subtract(z, v);
		float denominator_modulus = complex_modulus(denominator);
		
		// Check for division by 0 (return NEGATIVE to signal division detected)
		if (denominator_modulus < division_threshold) {
			smooth_value = 999999.0; // Marker for debug - will show as magenta
			return -1; // Return negative to signal division by zero
		}
		
		// z = (k / (z - v))^2 + x
		vec2 k_complex = vec2(k, 0.0);
		vec2 fraction = complex_divide(k_complex, denominator);
		z = complex_add(complex_square(fraction), x);
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

// Generate color from iteration count
// Special handling for division by 0 (the win condition!)
vec4 get_color(int iterations, float smooth_value) {
	// Division by 0 - WIN! Show bright red (negative iterations is marker)
	if (iterations < 0) {
		return vec4(1.0, 0.0, 0.0, 1.0); // Bright RED for win condition
	}
	
	// DEBUG: Show magenta if smooth_value is marker value (very high)
	if (smooth_value > float(max_iterations) * 2.0) {
		return vec4(1.0, 0.0, 1.0, 1.0); // MAGENTA debug marker
	}
	
	if (iterations == max_iterations) {
		return vec4(0.0, 0.0, 0.0, 1.0); // Black for in-set points
	}
	
	// Robust palette: use iteration ratio to create RGB bands
	float t = float(iterations) / float(max_iterations);
	float phase = t * color_range + color_shift;
	vec3 rgb = 0.5 + 0.5 * sin(6.2831853 * (phase + vec3(0.0, 0.33, 0.67)));
	
	return vec4(rgb, 1.0);
}

// ============================================================================
// Main Fragment Shader
// ============================================================================

void fragment() {
	// Convert fragment coordinates to normalized coordinates [0, 1]
	vec2 uv = UV;
	
	// Calculate viewport dimensions in complex plane using texture aspect
	float width = zoom;
	float height = zoom * (tex_h / tex_w);
	
	// Convert screen coordinates to complex plane coordinates
	vec2 complex_pos = vec2(
		uv.x * width + center_x - width / 2.0,
		(1.0 - uv.y) * height + center_y - height / 2.0
	);
	
	// Compute division by zero Julia set
	int iterations = 0;
	float smooth_value = 0.0;
	
	vec2 v = vec2(v_real, v_imag); // Player-controlled singularity
	vec2 x = vec2(c_real, c_imag); // Fixed Julia constant
	
	iterations = compute_division_julia(complex_pos, v, x, k_value, smooth_value);
	
	// Convert iterations to color (negative iterations = division by zero)
	vec4 outcol = get_color(iterations, smooth_value);
	
	ALBEDO = outcol.rgb;
	ALPHA = outcol.a;
}
