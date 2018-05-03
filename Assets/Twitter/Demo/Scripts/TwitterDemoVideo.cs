using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TwitterKit.Unity;

public class TwitterDemoVideo : MonoBehaviour
{
    [SerializeField] InputField messageInput;
    [SerializeField] InputField hashTagInput;
    [SerializeField] Text videoPathLabel;
    [SerializeField] Button pickVideoButton;
    [SerializeField] Button shareVideoButton;

    void Start()
    {
        Twitter.Init();
    }

    void OnEnable()
    {
        pickVideoButton.onClick.AddListener(OnClickPickVideoButton);
        shareVideoButton.onClick.AddListener(OnClickShareVideoButton);
        shareVideoButton.interactable = false;
        messageInput.onValueChanged.AddListener(OnInputAreaUpdate);
    }

    void OnDisable()
    {
        pickVideoButton.onClick.RemoveListener(OnClickPickVideoButton);
        shareVideoButton.onClick.RemoveListener(OnClickShareVideoButton);
        messageInput.onValueChanged.RemoveListener(OnInputAreaUpdate);
    }

    void OnInputAreaUpdate(string text)
    {
        shareVideoButton.interactable = IsShareButtonEnable();
    }

    void OnClickPickVideoButton()
    {
        Debug.LogFormat("OnClickPickVideoButton");
        var permission = NativeGallery.GetVideoFromGallery((string path) =>
        {
            Debug.LogFormat(path);
            videoPathLabel.text = path;
            shareVideoButton.interactable = IsShareButtonEnable();
        }, "Select Video");

        Debug.LogFormat("Result permission is: {0}", permission);
    }

    void OnClickShareVideoButton()
    {
        Debug.LogFormat("OnClickShareVideoButton");
        Twitter.LogIn(
            (TwitterSession session) =>
            {
                Debug.Log("Success login");
                Share(session);
            },
            (ApiError error) =>
            {
                Debug.LogError(error.message);
            }
        );
    }

    void Share(TwitterSession session)
    {
        var message = messageInput.text;
        var hashtags = GetHashTags(hashTagInput.text);
        var videoUri = videoPathLabel.text;
        Twitter.ComposeWithVideo(session, videoUri, message, hashtags,
            (string tweetId) =>
            {
                // Success callback
                Debug.LogFormat("Tweet success, tweetId: {0}", tweetId);
            },
            (ApiError error) =>
            {
                // Error callback
                Debug.LogError("Tweet Failed " + error.message);
            },
            () =>
            {
                // Cancel callback
                Debug.Log("Compose cancelled");
            });
    }

    bool IsShareButtonEnable()
    {
        return !string.IsNullOrEmpty(messageInput.text)
            && !string.IsNullOrEmpty(videoPathLabel.text);
    }

    string[] GetHashTags(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return new string[] { };
        }
        return text.Split(',', ' ')
            .Where(tag => !string.IsNullOrEmpty(tag)) // Filter blank
            .Select(tag => tag.StartsWith("#") ? tag : '#' + tag) // Add #
            .ToArray();
    }


}
