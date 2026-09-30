/*
 * File: Components.kt
 * Purpose: Shared Compose building blocks in the web client's design language - the dark
 *          gridded top bar with its scanner light, square buttons, hairline cards, labelled
 *          fields, status chips, page headers and notices - plus formatters.
 * Project: Smart Solar Microgrid Trading System - Android App
 * Module: SE4040 Enterprise Application Development - Assignment 1
 */

package com.solargrid.app.ui.common

import androidx.compose.animation.core.CubicBezierEasing
import androidx.compose.animation.core.RepeatMode
import androidx.compose.animation.core.animateFloat
import androidx.compose.animation.core.infiniteRepeatable
import androidx.compose.animation.core.rememberInfiniteTransition
import androidx.compose.animation.core.tween
import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.IntrinsicSize
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.ColumnScope
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxHeight
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.statusBarsPadding
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.OutlinedTextFieldDefaults
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.drawBehind
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.geometry.Size
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.RectangleShape
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.input.ImeAction
import androidx.compose.ui.text.input.KeyboardType
import androidx.compose.ui.text.input.PasswordVisualTransformation
import androidx.compose.ui.text.input.VisualTransformation
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.em
import androidx.compose.ui.unit.sp
import com.solargrid.app.domain.model.ReservationStatus
import com.solargrid.app.ui.theme.Border
import com.solargrid.app.ui.theme.BorderSoft
import com.solargrid.app.ui.theme.DmSans
import com.solargrid.app.ui.theme.ErrorBg
import com.solargrid.app.ui.theme.ErrorRed
import com.solargrid.app.ui.theme.GridLine
import com.solargrid.app.ui.theme.Ink
import com.solargrid.app.ui.theme.Muted
import com.solargrid.app.ui.theme.SpaceGrotesk
import com.solargrid.app.ui.theme.StatusApproved
import com.solargrid.app.ui.theme.StatusApprovedBg
import com.solargrid.app.ui.theme.StatusCancelled
import com.solargrid.app.ui.theme.StatusCancelledBg
import com.solargrid.app.ui.theme.StatusCompleted
import com.solargrid.app.ui.theme.StatusCompletedBg
import com.solargrid.app.ui.theme.StatusPending
import com.solargrid.app.ui.theme.StatusPendingBg
import com.solargrid.app.ui.theme.SuccessGreen
import com.solargrid.app.ui.theme.White
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter
import java.time.format.FormatStyle

private val dateTimeFormatter = DateTimeFormatter.ofLocalizedDateTime(FormatStyle.MEDIUM, FormatStyle.SHORT)

fun Instant.formatLocal(): String = dateTimeFormatter.format(atZone(ZoneId.systemDefault()))

// ---------------------------------------------------------------------------------------------
// Dark surfaces: grid texture, brand mark and the scanner light (same as the web navbar)
// ---------------------------------------------------------------------------------------------

/** Faint 52dp grid drawn behind a dark surface, like the web's sign-in panel and navbar. */
fun Modifier.gridBackground(cell: Dp = 52.dp): Modifier = this
    .background(Ink)
    .drawBehind {
        val step = cell.toPx()
        var x = 0f
        while (x < size.width) {
            drawLine(GridLine, Offset(x, 0f), Offset(x, size.height), strokeWidth = 1f)
            x += step
        }
        var y = 0f
        while (y < size.height) {
            drawLine(GridLine, Offset(0f, y), Offset(size.width, y), strokeWidth = 1f)
            y += step
        }
    }

/** A hairline with a soft white light sweeping end to end and back. */
@Composable
fun ScannerLine(modifier: Modifier = Modifier) {
    val transition = rememberInfiniteTransition(label = "scanner")
    val progress by transition.animateFloat(
        initialValue = 0f,
        targetValue = 1f,
        animationSpec = infiniteRepeatable(
            animation = tween(durationMillis = 3400, easing = CubicBezierEasing(0.37f, 0f, 0.63f, 1f)),
            repeatMode = RepeatMode.Reverse
        ),
        label = "scannerProgress"
    )
    Canvas(modifier.fillMaxWidth().height(1.dp)) {
        drawRect(Color.White.copy(alpha = 0.14f))
        val coreWidth = 200.dp.toPx()
        val haloWidth = 240.dp.toPx()
        val haloHeight = 11.dp.toPx()
        val coreX = progress * (size.width - coreWidth)
        val haloX = coreX - (haloWidth - coreWidth) / 2f

        // Soft halo around the light.
        drawOval(
            brush = Brush.radialGradient(
                colors = listOf(Color.White.copy(alpha = 0.22f), Color.White.copy(alpha = 0.06f), Color.Transparent),
                center = Offset(haloX + haloWidth / 2f, size.height / 2f),
                radius = haloWidth / 2f
            ),
            topLeft = Offset(haloX, size.height / 2f - haloHeight / 2f),
            size = Size(haloWidth, haloHeight)
        )
        // Bright core that fades out at both ends.
        drawRect(
            brush = Brush.horizontalGradient(
                colors = listOf(
                    Color.Transparent,
                    Color.White.copy(alpha = 0.35f),
                    Color.White,
                    Color.White.copy(alpha = 0.35f),
                    Color.Transparent
                ),
                startX = coreX,
                endX = coreX + coreWidth
            ),
            topLeft = Offset(coreX, 0f),
            size = Size(coreWidth, size.height)
        )
    }
}

/** The square "S" mark used by the web navbar and sign-in page. */
@Composable
fun BrandMark(size: Dp = 32.dp, inverted: Boolean = false) {
    Box(
        Modifier
            .size(size)
            .then(if (inverted) Modifier.background(White) else Modifier.border(1.dp, White.copy(alpha = 0.55f))),
        contentAlignment = Alignment.Center
    ) {
        Text(
            "S",
            color = if (inverted) Ink else White,
            fontFamily = SpaceGrotesk,
            fontWeight = FontWeight.SemiBold,
            fontSize = (size.value * 0.42f).sp
        )
    }
}

/**
 * Top bar: dark ink with the faint grid, the brand mark (or a back button) and a title, and the
 * scanner light along its bottom edge. Handles the status-bar inset itself.
 */
@Composable
fun SgTopBar(title: String, onBack: (() -> Unit)? = null, actions: @Composable RowScope.() -> Unit = {}) {
    Column(Modifier.fillMaxWidth().gridBackground()) {
        Row(
            Modifier.fillMaxWidth().statusBarsPadding().height(60.dp).padding(horizontal = 12.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            if (onBack != null) {
                IconButton(onClick = onBack) {
                    Icon(CircumIcons.Back, contentDescription = "Back", tint = White)
                }
            } else {
                Spacer(Modifier.width(4.dp))
                BrandMark(size = 30.dp)
                Spacer(Modifier.width(12.dp))
            }
            Text(
                title,
                modifier = Modifier.weight(1f),
                color = White,
                fontFamily = SpaceGrotesk,
                fontWeight = FontWeight.SemiBold,
                fontSize = 18.sp,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
            Row(verticalAlignment = Alignment.CenterVertically) {
                // Icons in the bar are white.
                CompositionLocalProvider(LocalContentColor provides White) { actions() }
            }
        }
        ScannerLine()
    }
}

// ---------------------------------------------------------------------------------------------
// Text styles
// ---------------------------------------------------------------------------------------------

/** Small uppercase label used above titles and fields (the web's "eyebrow"). */
@Composable
fun Eyebrow(text: String, modifier: Modifier = Modifier, color: Color = Muted) {
    Text(
        text.uppercase(),
        modifier = modifier,
        color = color,
        fontFamily = DmSans,
        fontWeight = FontWeight.Medium,
        fontSize = 11.sp,
        letterSpacing = 0.06.em
    )
}

/** Page heading: eyebrow, title, optional subtitle and a hairline divider - like the web PageHeader. */
@Composable
fun SgPageHeader(eyebrow: String, title: String, subtitle: String? = null, modifier: Modifier = Modifier) {
    Column(modifier.fillMaxWidth()) {
        Eyebrow(eyebrow)
        Text(title, style = MaterialTheme.typography.headlineSmall, modifier = Modifier.padding(top = 4.dp))
        if (subtitle != null) {
            Text(subtitle, style = MaterialTheme.typography.bodyMedium, color = Muted, modifier = Modifier.padding(top = 4.dp))
        }
        HorizontalDivider(Modifier.padding(top = 16.dp), color = Border)
    }
}

// ---------------------------------------------------------------------------------------------
// Buttons
// ---------------------------------------------------------------------------------------------

/** Primary action: square black button with an optional spinner. */
@Composable
fun LoadingButton(
    text: String,
    loading: Boolean,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
    icon: ImageVector? = null
) {
    Button(
        onClick = onClick,
        enabled = enabled && !loading,
        shape = RectangleShape,
        colors = ButtonDefaults.buttonColors(
            containerColor = Ink,
            contentColor = White,
            disabledContainerColor = Ink.copy(alpha = 0.55f),
            disabledContentColor = White
        ),
        modifier = modifier.fillMaxWidth().height(50.dp)
    ) {
        if (loading) {
            CircularProgressIndicator(modifier = Modifier.size(18.dp), strokeWidth = 2.dp, color = White)
            Spacer(Modifier.width(10.dp))
        } else if (icon != null) {
            Icon(icon, contentDescription = null, modifier = Modifier.size(20.dp))
            Spacer(Modifier.width(8.dp))
        }
        Text(text, style = MaterialTheme.typography.labelLarge)
    }
}

/** Secondary action: white button with a hairline border. */
@Composable
fun SgOutlinedButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
    contentColor: Color = Ink,
    icon: ImageVector? = null
) {
    OutlinedButton(
        onClick = onClick,
        enabled = enabled,
        shape = RectangleShape,
        border = BorderStroke(1.dp, if (contentColor == Ink) Border else contentColor.copy(alpha = 0.35f)),
        colors = ButtonDefaults.outlinedButtonColors(containerColor = White, contentColor = contentColor),
        modifier = modifier.fillMaxWidth().height(50.dp)
    ) {
        if (icon != null) {
            Icon(icon, contentDescription = null, modifier = Modifier.size(20.dp))
            Spacer(Modifier.width(8.dp))
        }
        Text(text, style = MaterialTheme.typography.labelLarge)
    }
}

// ---------------------------------------------------------------------------------------------
// Fields
// ---------------------------------------------------------------------------------------------

/** Square field with an uppercase label above it and an ink focus border. */
@Composable
fun SgTextField(
    label: String,
    value: String,
    onValueChange: (String) -> Unit,
    modifier: Modifier = Modifier,
    placeholder: String? = null,
    keyboardType: KeyboardType = KeyboardType.Text,
    imeAction: ImeAction = ImeAction.Next,
    secret: Boolean = false,
    enabled: Boolean = true,
    singleLine: Boolean = true
) {
    Column(modifier.fillMaxWidth()) {
        Eyebrow(label, modifier = Modifier.padding(bottom = 6.dp))
        OutlinedTextField(
            value = value,
            onValueChange = onValueChange,
            modifier = Modifier.fillMaxWidth(),
            enabled = enabled,
            singleLine = singleLine,
            placeholder = placeholder?.let { { Text(it, color = Muted.copy(alpha = 0.8f)) } },
            visualTransformation = if (secret) PasswordVisualTransformation() else VisualTransformation.None,
            keyboardOptions = KeyboardOptions(keyboardType = keyboardType, imeAction = imeAction),
            textStyle = MaterialTheme.typography.bodyLarge,
            shape = RectangleShape,
            colors = OutlinedTextFieldDefaults.colors(
                focusedBorderColor = Ink,
                unfocusedBorderColor = Border,
                disabledBorderColor = BorderSoft,
                focusedContainerColor = White,
                unfocusedContainerColor = White,
                disabledContainerColor = MaterialTheme.colorScheme.surfaceVariant,
                cursorColor = Ink
            )
        )
    }
}

// ---------------------------------------------------------------------------------------------
// Surfaces, chips and notices
// ---------------------------------------------------------------------------------------------

/** White card with a hairline border and no shadow. */
@Composable
fun SgCard(modifier: Modifier = Modifier, padding: Dp = 16.dp, content: @Composable ColumnScope.() -> Unit) {
    Column(
        modifier
            .fillMaxWidth()
            .background(White)
            .border(1.dp, Border)
            .padding(padding),
        content = content
    )
}

/** Square, uppercase, muted status badge - the same colours as the web badges. */
@Composable
fun StatusChip(status: ReservationStatus) {
    val (fg, bg) = when (status) {
        ReservationStatus.Pending -> StatusPending to StatusPendingBg
        ReservationStatus.Approved -> StatusApproved to StatusApprovedBg
        ReservationStatus.Cancelled -> StatusCancelled to StatusCancelledBg
        ReservationStatus.Completed -> StatusCompleted to StatusCompletedBg
    }
    Box(Modifier.background(bg).padding(horizontal = 8.dp, vertical = 4.dp)) {
        Text(
            status.name.uppercase(),
            color = fg,
            fontFamily = DmSans,
            fontWeight = FontWeight.Medium,
            fontSize = 10.5.sp,
            letterSpacing = 0.05.em
        )
    }
}

/** Error notice: red text on a pale background with a red left edge (web .notice-error). */
@Composable
fun ErrorText(message: String?) {
    if (message != null) Notice(message, fg = ErrorRed, bg = ErrorBg)
}

/** Success notice in the same style. */
@Composable
fun SuccessText(message: String?) {
    if (message != null) Notice(message, fg = SuccessGreen, bg = StatusApprovedBg)
}

@Composable
private fun Notice(message: String, fg: Color, bg: Color) {
    Row(Modifier.fillMaxWidth().padding(vertical = 4.dp).height(IntrinsicSize.Min).background(bg)) {
        Box(Modifier.width(3.dp).fillMaxHeight().background(fg))
        Text(message, color = fg, style = MaterialTheme.typography.bodyMedium, modifier = Modifier.padding(horizontal = 12.dp, vertical = 10.dp))
    }
}

@Composable
fun LabelValue(label: String, value: String) {
    Row(Modifier.fillMaxWidth().padding(vertical = 6.dp), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(label, color = Muted, style = MaterialTheme.typography.bodyMedium)
        Text(value, style = MaterialTheme.typography.bodyMedium, fontWeight = FontWeight.Medium)
    }
}

@Composable
fun CenteredLoading() {
    Box(Modifier.fillMaxSize(), contentAlignment = Alignment.Center) { CircularProgressIndicator(color = Ink) }
}

@Composable
fun EmptyState(text: String) {
    Box(Modifier.fillMaxWidth().padding(32.dp), contentAlignment = Alignment.Center) {
        Text(text, color = Muted, style = MaterialTheme.typography.bodyMedium)
    }
}

/** Underlined text tabs, like the web's pill tabs on the Prosumers page. */
@Composable
fun SgTabRow(tabs: List<String>, selected: Int, onSelect: (Int) -> Unit, modifier: Modifier = Modifier) {
    Column(modifier.fillMaxWidth()) {
        Row(Modifier.fillMaxWidth()) {
            tabs.forEachIndexed { index, label ->
                val active = index == selected
                Column(
                    Modifier.weight(1f).clickable { onSelect(index) },
                    horizontalAlignment = Alignment.CenterHorizontally
                ) {
                    Eyebrow(label, color = if (active) Ink else Muted, modifier = Modifier.padding(vertical = 14.dp))
                    Box(Modifier.fillMaxWidth().height(2.dp).background(if (active) Ink else Color.Transparent))
                }
            }
        }
        HorizontalDivider(color = Border)
    }
}

/** Square confirmation dialog with text actions in the app's type style. */
@Composable
fun SgConfirmDialog(
    title: String,
    text: String,
    confirmLabel: String,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit,
    dismissLabel: String = "Cancel",
    destructive: Boolean = false
) {
    AlertDialog(
        onDismissRequest = onDismiss,
        shape = RectangleShape,
        containerColor = White,
        title = { Text(title, style = MaterialTheme.typography.titleLarge) },
        text = { Text(text, style = MaterialTheme.typography.bodyMedium, color = Muted) },
        confirmButton = {
            TextButton(onClick = onConfirm, shape = RectangleShape) {
                Text(confirmLabel, color = if (destructive) ErrorRed else Ink, style = MaterialTheme.typography.labelLarge)
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss, shape = RectangleShape) {
                Text(dismissLabel, color = Muted, style = MaterialTheme.typography.labelLarge)
            }
        }
    )
}
