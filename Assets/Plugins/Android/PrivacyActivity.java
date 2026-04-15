package com.Interaforeqe.skater;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.Uri;
import android.os.Bundle;
import android.text.SpannableString;
import android.text.Spanned;
import android.text.method.LinkMovementMethod;
import android.text.style.ClickableSpan;
import android.view.View;
import android.widget.TextView;

public class PrivacyActivity extends Activity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        SharedPreferences prefs = getSharedPreferences("privacy", MODE_PRIVATE);
        boolean agreed = prefs.getBoolean("agreed", false);

        if (agreed) {
            startUnity();
            return;
        }

        String fullText = "我们重视您的隐私。本游戏使用Unity引擎，启动时会自动收集设备标识符（AndroidID）、设备型号、操作系统版本等信息，仅用于游戏运行适配，不进行联网传输。\n\n点击\"同意\"即表示您已阅读并同意《隐私政策》。";
        
        SpannableString spannable = new SpannableString(fullText);
        int start = fullText.indexOf("《隐私政策》");
        int end = start + "《隐私政策》".length();
        spannable.setSpan(new ClickableSpan() {
            @Override
            public void onClick(View view) {
                Intent browser = new Intent(Intent.ACTION_VIEW, Uri.parse("https://skater-privacy.pages.dev/privacy"));
                startActivity(browser);
            }
        }, start, end, Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);

        AlertDialog dialog = new AlertDialog.Builder(this)
            .setTitle("隐私政策")
            .setMessage(spannable)
            .setCancelable(false)
            .setPositiveButton("同意", new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int which) {
                    prefs.edit().putBoolean("agreed", true).apply();
                    startUnity();
                }
            })
            .setNegativeButton("不同意", new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int which) {
                    finish();
                }
            })
            .create();

        dialog.show();

        // 让链接可点击
        TextView messageView = dialog.findViewById(android.R.id.message);
        if (messageView != null) {
            messageView.setMovementMethod(LinkMovementMethod.getInstance());
        }
    }

    private void startUnity() {
        Intent intent = new Intent(this, com.unity3d.player.UnityPlayerActivity.class);
        startActivity(intent);
        finish();
    }
}